using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using FastReport.Utils;

namespace FastReport.Engine
{
  internal enum EngineState
  {
    ReportStarted,
    ReportFinished,
    ReportPageStarted,
    ReportPageFinished,
    PageStarted,
    PageFinished,
    ColumnStarted,
    ColumnFinished,
    BlockStarted,
    BlockFinished,
    GroupStarted,
    GroupFinished
  }

  internal class EngineStateChangedEventArgs
  {
    private ReportEngine FEngine;
    private EngineState FState;

    public ReportEngine Engine
    {
      get { return FEngine; }
    }

    public EngineState State
    {
      get { return FState; }
    }

    internal EngineStateChangedEventArgs(ReportEngine engine, EngineState state)
    {
      FEngine = engine;
      FState = state;
    }
  }

  internal delegate void EngineStateChangedEventHandler(object sender, EngineStateChangedEventArgs e);

  internal class ProcessInfo
  {
    private TextObjectBase textObject;
    private XmlItem xmlItem;

    public bool Process(object sender, EngineState state)
    {
      ProcessAt processAt = textObject.ProcessAt;
      bool canProcess = false;

      if ((processAt == ProcessAt.DataFinished && state == EngineState.BlockFinished) ||
        (processAt == ProcessAt.GroupFinished && state == EngineState.GroupFinished))
      {
        // check which data is finished
        BandBase topParentBand = textObject.Band;
        if (topParentBand is ChildBand)
          topParentBand = (topParentBand as ChildBand).GetTopParentBand;

        if (processAt == ProcessAt.DataFinished && state == EngineState.BlockFinished)
        {
          // total can be printed on the same data header, or on its parent data band
          DataBand senderBand = sender as DataBand;
          canProcess = true;
          if (topParentBand is DataHeaderBand && (topParentBand.Parent != sender))
            canProcess = false;
          if (topParentBand is DataBand && senderBand.Parent != topParentBand)
            canProcess = false;
        }
        else
        {
          // total can be printed on the same group header
          canProcess = sender == topParentBand;
        }
      }
      else
      {
        canProcess = (processAt == ProcessAt.ReportFinished && state == EngineState.ReportFinished) ||
          (processAt == ProcessAt.ReportPageFinished && state == EngineState.ReportPageFinished) ||
          (processAt == ProcessAt.PageFinished && state == EngineState.PageFinished) ||
          (processAt == ProcessAt.ColumnFinished && state == EngineState.ColumnFinished);
      }

      if (canProcess)
      {
        textObject.SaveState();
        try
        {
          textObject.GetData();
          xmlItem.SetProp("x", textObject.Text);
        }
        finally
        {
          textObject.RestoreState();
        }
        return true;
      }
      else
        return false;
    }

    public ProcessInfo(TextObjectBase obj, XmlItem item)
    {
      textObject = obj;
      xmlItem = item;
    }
  }

  public partial class ReportEngine
  {
    private List<ProcessInfo> FObjectsToProcess;
    internal event EngineStateChangedEventHandler StateChanged;

    private void OnStateChanged(object sender, EngineState state)
    {
      ProcessObjects(sender, state);
      if (StateChanged != null)
        StateChanged(sender, new EngineStateChangedEventArgs(this, state));
    }

    private void ProcessObjects(object sender, EngineState state)
    {
      for (int i = 0; i < FObjectsToProcess.Count; i++)
      {
        ProcessInfo info = FObjectsToProcess[i];
        if (info.Process(sender, state))
        {
          FObjectsToProcess.RemoveAt(i);
          i--;
        }
      }
    }

    internal void AddObjectToProcess(Base obj, XmlItem item)
    {
      TextObjectBase textObj = obj as TextObjectBase;
      if (textObj == null || textObj.ProcessAt == ProcessAt.Default)
        return;

      FObjectsToProcess.Add(new ProcessInfo(textObj, item));
    }
  }
}
