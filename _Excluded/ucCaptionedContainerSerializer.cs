using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.Design.Serialization;
using System.CodeDom;
using System.Windows.Forms;

namespace KGySoft.Controls.Design
{
	// TODO: BUG: Despite of the foreach, only the first child is assigned to PanelContent
	/// <summary>
	/// Serializes <see cref="ucCaptionedContainer"/> along with its children.
	/// </summary>
	internal class ucCaptionedContainerSerializer: CodeDomSerializer
	{
		public override object Serialize(IDesignerSerializationManager manager, object value)
		{
			// serializing base type normally
			CodeDomSerializer baseSerializer = manager.GetSerializer(typeof(ucCaptionedBase), typeof(CodeDomSerializer)) as CodeDomSerializer;
			object result = baseSerializer.Serialize(manager, value);

			ucCaptionedContainer toSerialize = value as ucCaptionedContainer;

			if (result is CodeStatementCollection && toSerialize.Site != null)
			{
				CodeStatementCollection statements = result as CodeStatementCollection;

				// retrieveing PanelContent property of the user control
				CodePropertyReferenceExpression propPanelContent = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), toSerialize.Site.Name), "PanelContent");

				// assigning Parent property of each child with PanelContent
				foreach (Control c in toSerialize.PanelContent.Controls)
				{
					//  retrieveing Parent property of the child
					CodePropertyReferenceExpression propParent = new CodePropertyReferenceExpression(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), c.Name), "Parent");

					// parenting the child into PanelContent
					statements.Add(new CodeAssignStatement(propParent, propPanelContent));
				}
			}

			return result;
		}

	}
}
