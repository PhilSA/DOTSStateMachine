using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Reflection;
using System.Linq;

public static class StateMachineWizard
{
    [MenuItem("Tools/Generate StateMachines")]
    public static void Generate()
    {

        // search project for all interfaces with StateMachineInterface attribute
        List<Type> stateDefinitionInterfaces = ScanInterfaceTypesWithAttributes(typeof(StateMachineDefinition));
        foreach (Type interfaceType in stateDefinitionInterfaces)
        {
            List<Type> stateImplementations = ScanStructTypesImplementingInterface(interfaceType);

            StateMachineDefinition stateDefinitionAttribute = (StateMachineDefinition)Attribute.GetCustomAttribute(interfaceType, typeof(StateMachineDefinition));

            string folderPath = Application.dataPath + "/" + stateDefinitionAttribute.FilePathRelativeToAssets;
            Directory.CreateDirectory(folderPath);

            int indentLevel = 0;
            StreamWriter writer = File.CreateText(folderPath + "/" + stateDefinitionAttribute.ComponentName + ".cs");

            writer.WriteLine(GetIndent(indentLevel) + "using System;");
            writer.WriteLine(GetIndent(indentLevel) + "using Unity.Entities;");
            writer.WriteLine(GetIndent(indentLevel) + "using Unity.Mathematics;");
            if (stateDefinitionAttribute.AdditionalUsings != null)
            {
                foreach (string addUsing in stateDefinitionAttribute.AdditionalUsings)
                {
                    writer.WriteLine(GetIndent(indentLevel) + "using " + addUsing + ";");
                }
            }

            writer.WriteLine("");

            if (!string.IsNullOrEmpty(interfaceType.Namespace))
            {
                writer.WriteLine(GetIndent(indentLevel) + "namespace " + interfaceType.Namespace);
                writer.WriteLine(GetIndent(indentLevel) + "{");
                indentLevel++;
            }
            {
                // Generate statemachine component
                writer.WriteLine(GetIndent(indentLevel) + "[Serializable]");
                writer.WriteLine(GetIndent(indentLevel) + "public struct " + stateDefinitionAttribute.ComponentName + " : IComponentData");
                writer.WriteLine(GetIndent(indentLevel) + "{");
                indentLevel++;
                {
                    MethodInfo[] stateMethods = interfaceType.GetMethods();

                    // Generate states enum
                    writer.WriteLine(GetIndent(indentLevel) + "public enum State");
                    writer.WriteLine(GetIndent(indentLevel) + "{");
                    indentLevel++;
                    {
                        foreach (Type stateType in stateImplementations)
                        {
                            writer.WriteLine(GetIndent(indentLevel) + stateType.Name + ",");
                        }
                    }
                    indentLevel--;
                    writer.WriteLine(GetIndent(indentLevel) + "}");

                    writer.WriteLine("");

                    writer.WriteLine(GetIndent(indentLevel) + "public State CurrentState;");
                    writer.WriteLine(GetIndent(indentLevel) + "public State PreviousState;");

                    writer.WriteLine("");

                    // Generate all state fields
                    foreach (Type stateType in stateImplementations)
                    {
                        writer.WriteLine(GetIndent(indentLevel) + "public " + stateType.Name + " " + stateType.Name + ";");
                    }

                    writer.WriteLine("");

                    // Generate state transition function
                    {
                        MethodInfo stateEnterMethod = null;
                        MethodInfo stateExitMethod = null;
                        List<ParameterInfo> allEnterExitParams = new List<ParameterInfo>();
                        foreach (MethodInfo method in stateMethods)
                        {
                            if (method.Name == "OnStateEnter")
                            {
                                stateEnterMethod = method;
                                allEnterExitParams.AddRange(method.GetParameters());
                            }
                            if (method.Name == "OnStateExit")
                            {
                                stateExitMethod = method;
                                allEnterExitParams.AddRange(method.GetParameters());
                            }
                        }

                        // Filter all params for unique ones
                        for (int i = allEnterExitParams.Count - 1; i >= 0; i--)
                        {
                            for (int j = 0; j < i; j++)
                            {
                                if (allEnterExitParams[i].Name == allEnterExitParams[j].Name)
                                {
                                    // prioritize ref over in/out
                                    if(IsByRef(allEnterExitParams[i]) && !IsByRef(allEnterExitParams[j]))
                                    {
                                        allEnterExitParams[j] = allEnterExitParams[i];
                                    }

                                    allEnterExitParams.RemoveAt(i);
                                    break;
                                }
                            }
                        }

                        string methodDeclaration = "public void TransitionTo(State newState";
                        for (int i = 0; i < allEnterExitParams.Count; i++)
                        {
                            ParameterInfo paramInfo = allEnterExitParams[i];
                            methodDeclaration += ", ";
                            methodDeclaration += GetParameterDecorator(paramInfo) + paramInfo.ParameterType.Name.Replace("&", "") + " " + paramInfo.Name;
                        }
                        methodDeclaration += ", bool force = false)";

                        writer.WriteLine(GetIndent(indentLevel) + methodDeclaration);
                        writer.WriteLine(GetIndent(indentLevel) + "{");
                        indentLevel++;
                        {
                            // init all out parameters to default
                            foreach (var param in allEnterExitParams)
                            {
                                if (param.IsOut)
                                {
                                    writer.WriteLine(GetIndent(indentLevel) + param.Name + " = default;");
                                }
                            }

                            writer.WriteLine(GetIndent(indentLevel) + "if (force || CurrentState != newState)");
                            writer.WriteLine(GetIndent(indentLevel) + "{");
                            indentLevel++;
                            {
                                writer.WriteLine(GetIndent(indentLevel) + "PreviousState = CurrentState;");
                                writer.WriteLine(GetIndent(indentLevel) + "CurrentState = newState;");

                                if (stateExitMethod != null)
                                {
                                    string methodCallDeclaration = "OnStateExit(PreviousState";
                                    var exitParameters = stateExitMethod.GetParameters();
                                    for (int i = 0; i < exitParameters.Length; i++)
                                    {
                                        ParameterInfo paramInfo = exitParameters[i];
                                        methodCallDeclaration += ", ";
                                        methodCallDeclaration += GetParameterDecorator(paramInfo) + paramInfo.Name;
                                    }
                                    methodCallDeclaration += ");";

                                    writer.WriteLine(GetIndent(indentLevel) + methodCallDeclaration);
                                }

                                if (stateEnterMethod != null)
                                {
                                    string methodCallDeclaration = "OnStateEnter(CurrentState";
                                    var enterParameters = stateEnterMethod.GetParameters();
                                    for (int i = 0; i < enterParameters.Length; i++)
                                    {
                                        ParameterInfo paramInfo = enterParameters[i];
                                        methodCallDeclaration += ", ";
                                        methodCallDeclaration += GetParameterDecorator(paramInfo) + paramInfo.Name;
                                    }
                                    methodCallDeclaration += ");";

                                    writer.WriteLine(GetIndent(indentLevel) + methodCallDeclaration);
                                }
                            }
                            indentLevel--;
                            writer.WriteLine(GetIndent(indentLevel) + "}");
                        }
                        indentLevel--;
                        writer.WriteLine(GetIndent(indentLevel) + "}");
                    }

                    // Generate all state functions
                    foreach (MethodInfo stateMethod in stateMethods)
                    {
                        writer.WriteLine("");

                        ParameterInfo[] methodParameters = stateMethod.GetParameters();
                        string methodDeclaration = "public void " + stateMethod.Name + "(State state";
                        for (int i = 0; i < methodParameters.Length; i++)
                        {
                            ParameterInfo paramInfo = methodParameters[i];
                            methodDeclaration += ", ";
                            methodDeclaration += GetParameterDecorator(paramInfo) + paramInfo.ParameterType.Name.Replace("&", "") + " " + paramInfo.Name;
                        }
                        methodDeclaration += ")";

                        writer.WriteLine(GetIndent(indentLevel) + methodDeclaration);
                        writer.WriteLine(GetIndent(indentLevel) + "{");
                        indentLevel++;
                        {
                            // init all out parameters to default
                            foreach (var param in methodParameters)
                            {
                                if(param.IsOut)
                                {
                                    writer.WriteLine(GetIndent(indentLevel) + param.Name + " = default;");
                                }
                            }

                            // Generate the switch case
                            writer.WriteLine(GetIndent(indentLevel) + "switch (state)");
                            writer.WriteLine(GetIndent(indentLevel) + "{");
                            indentLevel++;
                            {
                                foreach (Type stateType in stateImplementations)
                                {
                                    writer.WriteLine(GetIndent(indentLevel) + "case State." + stateType.Name + ":");

                                    indentLevel++;

                                    string methodCallDeclaration = stateMethod.Name + "(";
                                    for (int i = 0; i < methodParameters.Length; i++)
                                    {
                                        ParameterInfo paramInfo = methodParameters[i];
                                        if (i > 0)
                                        {
                                            methodCallDeclaration += ", ";
                                        }
                                        methodCallDeclaration += GetParameterDecorator(paramInfo) + paramInfo.Name;
                                    }
                                    methodCallDeclaration += ");";

                                    writer.WriteLine(GetIndent(indentLevel) + stateType.Name + "." + methodCallDeclaration);

                                    writer.WriteLine(GetIndent(indentLevel) + "break;");

                                    indentLevel--;
                                }
                            }
                            indentLevel--;
                            writer.WriteLine(GetIndent(indentLevel) + "}");
                        }
                        indentLevel--;
                        writer.WriteLine(GetIndent(indentLevel) + "}");
                    }
                }
                indentLevel--;
                writer.WriteLine(GetIndent(indentLevel) + "}");
            }
            if (!string.IsNullOrEmpty(interfaceType.Namespace))
            {
                indentLevel--;
                writer.WriteLine(GetIndent(indentLevel) + "}");
            }

            writer.Close();
        }

        AssetDatabase.Refresh();
    }

    public static bool IsByRef(ParameterInfo parameterInfo)
    {
        return parameterInfo.ParameterType.IsByRef && !parameterInfo.IsOut && !parameterInfo.IsIn;
    }

    private static string GetParameterDecorator(ParameterInfo parameterInfo)
    {
        string s = "";
        if(parameterInfo.ParameterType.IsByRef)
        {
            if (parameterInfo.IsOut)
            {
                s += "out ";
            }
            else if (parameterInfo.IsIn)
            {
                s += "in ";
            }
            else
            {
                s += "ref ";
            }
        }

        return s;
    }

    private static string GetIndent(int indentationLevel)
    {
        string indentation = "";
        for (int i = 0; i < indentationLevel; i++)
        {
            indentation += "\t";
        }
        return indentation;
    }

    static List<Type> ScanInterfaceTypesWithAttributes(Type attributeType)
    {
        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            Type[] allAssemblyTypes;
            try
            {
                allAssemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                allAssemblyTypes = e.Types;
            }
            var myTypes = allAssemblyTypes.Where(t => t.IsInterface && Attribute.IsDefined(t, attributeType, true));
            types.AddRange(myTypes);
        }
        return types;
    }

    static List<Type> ScanStructTypesImplementingInterface(Type interfaceType)
    {
        var types = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            Type[] allAssemblyTypes;
            try
            {
                allAssemblyTypes = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                allAssemblyTypes = e.Types;
            }

            var myTypes = allAssemblyTypes.Where(t => t.IsValueType && interfaceType.IsAssignableFrom(t));
            types.AddRange(myTypes);
        }
        return types;
    }
}