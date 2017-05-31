using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using LM.ImageComments.EditorComponent;
using System.Diagnostics;

namespace LM.ImageComments
{
    [Export(typeof(IDropHandlerProvider))]
    [DropFormat(ImageCommentDropHandlerProvider.FileDropDataFormat)]
    [DropFormat(ImageCommentDropHandlerProvider.VSProjectItemDataFormat)]
    [Name("Image Comment Drop Handler")]
    [Order(Before = "DefaultFileDropHandler")]
    internal class ImageCommentDropHandlerProvider : IDropHandlerProvider
    {
        internal const string VSProjectItemDataFormat = "CF_VSSTGPROJECTITEMS";
        internal const string FileDropDataFormat = "FileDrop";

        public IDropHandler GetAssociatedDropHandler(IWpfTextView view)
        {
            try
            {
                ImageAdornmentManager imagesManager = view.Properties.GetProperty<ImageAdornmentManager>(typeof(ImageAdornmentManager));

                return view.Properties.GetOrCreateSingletonProperty<ImageCommentDropHandler>(() => new ImageCommentDropHandler(imagesManager));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            return null;
        }
    }

}
