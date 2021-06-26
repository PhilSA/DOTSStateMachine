using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Interface)]
public class StateMachineDefinition : System.Attribute
{
    public string ComponentName;
    public string FilePathRelativeToAssets;
    public string[] AdditionalUsings;

    public StateMachineDefinition(string stateMachineComponentName, string stateMachineFilePathRelativeToAssets = "/_GENERATED/StateMachine", string[] additionalUsings = null)
    {
        ComponentName = stateMachineComponentName;
        FilePathRelativeToAssets = stateMachineFilePathRelativeToAssets;
        AdditionalUsings = additionalUsings;
    }
}