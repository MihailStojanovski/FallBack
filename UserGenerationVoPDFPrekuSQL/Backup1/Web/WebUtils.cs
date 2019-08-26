using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI.Design;
using System.ComponentModel.Design;

namespace FastReport.Web
{
  internal static class WebUtils
  {
    public static bool IsAbsolutePhysicalPath(string path)
    {
      if ((path == null) || (path.Length < 3))
      {
        return false;
      }
      return (path.StartsWith(@"\\", StringComparison.Ordinal) || ((char.IsLetter(path[0]) && (path[1] == ':')) && (path[2] == '\\')));
    }

    public static string MapPath(IServiceProvider serviceProvider, string path)
    {
      if (path.Length != 0)
      {
        if (IsAbsolutePhysicalPath(path))
        {
          return path;
        }
        WebFormsRootDesigner designer = null;
        if (serviceProvider != null)
        {
          IDesignerHost service = (IDesignerHost)serviceProvider.GetService(typeof(IDesignerHost));
          if ((service != null) && (service.RootComponent != null))
          {
            designer = service.GetDesigner(service.RootComponent) as WebFormsRootDesigner;
            if (designer != null)
            {
              string appRelativeUrl = designer.ResolveUrl(path);
              IWebApplication application = (IWebApplication)serviceProvider.GetService(typeof(IWebApplication));
              if (application != null)
              {
                IProjectItem projectItemFromUrl = application.GetProjectItemFromUrl(appRelativeUrl);
                if (projectItemFromUrl != null)
                {
                  return projectItemFromUrl.PhysicalPath;
                }
              }
            }
          }
        }
      }
      return null;
    }

  }
}
