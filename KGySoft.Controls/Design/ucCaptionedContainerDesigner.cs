using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms.Design;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.ComponentModel.Design;
using System.Drawing;

namespace KGySoft.Controls.Design
{
	/// <summary>
    /// Designer of <see cref="ucCaptionedContainer"/> that makes possible to use <see cref="ucCaptionedContainer"/>
    /// as a container control in design time.
	/// </summary>
    internal sealed class ucCaptionedContainerDesigner: ParentControlDesigner
	{
		IDesignerHost designerHost;

		public override void Initialize(IComponent component)
		{
            if (!(component is ucCaptionedContainer))
				throw new InvalidOperationException("The ucCaptionedContainerDesigner can be used only for user controls that are derived from ucCaptionedBase class.");
			base.Initialize(component);
			base.AutoResizeHandles = true;
			base.EnableDesignMode((component as ucCaptionedContainer).PanelContent, "ContentPanel");
			designerHost = (IDesignerHost)component.Site.GetService(typeof(IDesignerHost));
		}

		public override bool CanParent(Control control)
		{
			return false;
		}

		public override System.Collections.ICollection AssociatedComponents
		{
			get
			{
				List<Control> list = new List<Control>();
				foreach (Control control in ((ucCaptionedContainer)Control).PanelContent.Controls)
				{
					list.Add(control);
				}
				return list;

			}
		}

		protected override Control GetParentForComponent(IComponent component)
		{
			return ((ucCaptionedContainer)Control).PanelContent;
		}

		public override int NumberOfInternalControlDesigners()
		{
			return 1;
		}

		public override ControlDesigner InternalControlDesigner(int internalControlIndex)
		{
			Control panel = ((ucCaptionedContainer)Control).PanelContent;
			switch (internalControlIndex)
			{
				case 0:
					return this.designerHost.GetDesigner(panel) as ControlDesigner;
				default:
					return null;
			}
		}

		protected override IComponent[] CreateToolCore(ToolboxItem tool, int x, int y, int width, int height, bool hasLocation, bool hasSize)
		{
			ParentControlDesigner panelDesigner = this.designerHost.GetDesigner(((ucCaptionedContainer)Control).PanelContent) as ParentControlDesigner;
			InvokeCreateTool(panelDesigner, tool);
			return null;
		}
	}
}
