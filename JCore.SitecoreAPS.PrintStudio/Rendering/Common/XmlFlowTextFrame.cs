// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlFlowTextFrame.cs" company="Sitecore Corporation">
//   Copyright (C) 2013 by Sitecore Corporation
// </copyright>
// <summary>
//   Renders a text frame contents.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace JCore.SitecoreAPS.PrintStudio.Rendering.Common
{
  using System.Xml.Linq;
  using Sitecore.PrintStudio.PublishingEngine;
  using Sitecore.PrintStudio.PublishingEngine.Rendering;

  /// <summary>
  /// Renders a text frame contents.
  /// </summary>
  public class XmlFlowTextFrame : XmlTextFrameRenderer
  {
    /// <summary>
    /// The render content.
    /// </summary>
    /// <param name="printContext">
    /// The print context.
    /// </param>
    /// <param name="output">
    /// The output.
    /// </param>
    protected override void RenderContent(PrintContext printContext, XElement output)
    {
      XElement baseXml = new XElement("base");
      base.RenderContent(printContext, baseXml);

      XElement textFrame = baseXml.Element("TextFrame");
      if (textFrame != null)
      {
          output.Add(textFrame.Elements());
      }
    }
  }
}
