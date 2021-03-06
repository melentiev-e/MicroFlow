﻿using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace MicroFlow.Meta
{
  public class ReferencesCollector : INodeVisitor
  {
    private static readonly MethodInfo AssemblyLoadByName =
      typeof(Assembly)
        .GetRuntimeMethod("Load", new[] { typeof(string) });

    private HashSet<Assembly> myAssemblies;

    public HashSet<Assembly> Collect(FlowScheme scheme)
    {
      myAssemblies = new HashSet<Assembly>
      {
        typeof(object).GetTypeInfo().Assembly,
        typeof(Enumerable).GetTypeInfo().Assembly,
        typeof(Expression<>).GetTypeInfo().Assembly,
        typeof(Flow).GetTypeInfo().Assembly
      };

      var systemRuntime = (Assembly)AssemblyLoadByName.Invoke(null,
        new object[] { "System.Runtime, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" });

      if (systemRuntime != null)
      {
        myAssemblies.Add(systemRuntime);
      }

      var linqExpressions = (Assembly)AssemblyLoadByName.Invoke(null,
        new object[] { "System.Linq.Expressions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" });

      if (linqExpressions != null)
      {
        myAssemblies.Add(linqExpressions);
      }

      foreach (var node in scheme.Nodes)
      {
        node.Accept(this);
      }

      foreach (var variable in scheme.GlobalVariables)
      {
        myAssemblies.Add(variable.Type.GetTypeInfo().Assembly);
      }

      return myAssemblies;
    }

    public void Visit(ActivityInfo node)
    {
      myAssemblies.Add(node.ActivityType.GetTypeInfo().Assembly);
    }

    public void Visit(ConditionInfo node)
    {
    }

    public void Visit(SwitchInfo node)
    {
      myAssemblies.Add(node.Type.GetTypeInfo().Assembly);
    }

    public void Visit(ForkJoinInfo node)
    {
      foreach (var fork in node.Forks)
      {
        Visit(fork);
      }
    }

    public void Visit(BlockInfo node)
    {
      foreach (var innerNode in node.Nodes)
      {
        innerNode.Accept(this);
      }

      if (node.Variables != null)
      {
        foreach (var variable in node.Variables)
        {
          myAssemblies.Add(variable.Type.GetTypeInfo().Assembly);
        }
      }
    }
  }
}