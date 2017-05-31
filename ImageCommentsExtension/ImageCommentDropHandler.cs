using LM.ImageComments.EditorComponent;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using Microsoft.VisualStudio.Text.Formatting;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LM.ImageComments
{
    /// <summary>
    /// Handles a drag and drop of an image onto the editor.
    /// The image came from the file system (FileDrop) or from the VS Solution Explorer.
    /// </summary>
    internal class ImageCommentDropHandler : IDropHandler
    {
        private ImageAdornmentManager manager;
        private readonly List<string> SupportedImageExtensions = new List<string> { ".jpg", ".jpeg", ".bmp", ".png", ".gif" };

        internal ImageCommentDropHandler(ImageAdornmentManager manager)
        {
            this.manager = manager;
        }

        /// <summary>
        /// See <see cref="IDropHandler.HandleDragStarted"/> for more information.
        /// </summary>
        /// <param name="dragDropInfo"></param>
        /// <returns></returns>
        public DragDropPointerEffects HandleDragStarted(DragDropInfo dragDropInfo)
        {
            //drag started, so create a new Bitmap to be shown to the user as visual feedback
            //string imageFilename = GetImageFilename(dragDropInfo);

            //this.manager.PreviewImageAdornment.Show(imageFilename);

            //show the copy cursor to the user
            return DragDropPointerEffects.Link | DragDropPointerEffects.Track;
        }

        /// <summary>
        /// See <see cref="IDropHandler.HandleDraggingOver"/> for more information.
        /// </summary>
        /// <param name="dragDropInfo"></param>
        /// <returns></returns>
        public DragDropPointerEffects HandleDraggingOver(DragDropInfo dragDropInfo)
        {
            try
            {
                ITextViewLine targetLine = manager.GetTargetTextViewLine(dragDropInfo);
                if (targetLine != null )// && targetLine.Length > 0 && manager.HasMatch(targetLine.Extent.GetText()))
                {
                    return DragDropPointerEffects.Link | DragDropPointerEffects.Track;
                }
                else
                {
                    return DragDropPointerEffects.None;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return DragDropPointerEffects.None;
            }
        }

        //private void RemovePreviewImage()
        //{
        //    //this.manager.PreviewImageAdornment.Clear();
        //    //this.manager.HighlightLineAdornment.Clear();
        //}

        /// <summary>
        /// See <see cref="IDropHandler.HandleDataDropped"/> for more information.
        /// </summary>
        /// <param name="dragDropInfo"></param>
        /// <returns></returns>
        public DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo)
        {
            try
            {
                
                manager.InsertImageLinkAt(GetImageFilename(dragDropInfo) , dragDropInfo.VirtualBufferPosition);
                return DragDropPointerEffects.Link | DragDropPointerEffects.Track;
            }
            catch(ArgumentOutOfRangeException e)
            {
                Debug.WriteLine(e.Message);
                return DragDropPointerEffects.None;
            }
        }

        /// <summary>
        /// See <see cref="IDropHandler.IsDropEnabled"/> for more information.
        /// </summary>
        /// <param name="dragDropInfo"></param>
        /// <returns></returns>
        public bool IsDropEnabled(DragDropInfo dragDropInfo)
        {
            bool result = false;

            string imageFilename = GetImageFilename(dragDropInfo);

            if (!string.IsNullOrEmpty(imageFilename))
            {
                string imageFileExtension = Path.GetExtension(imageFilename).ToLowerInvariant();
                result = this.SupportedImageExtensions.Contains(imageFileExtension);
            }

            return result;
        }

        private static string GetImageFilename(DragDropInfo info)
        {
            DataObject data = new DataObject(info.Data);

            if (info.Data.GetDataPresent(ImageCommentDropHandlerProvider.FileDropDataFormat))
            {
                // The drag and drop operation came from the file system
                StringCollection files = data.GetFileDropList();

                if (files != null && files.Count == 1)
                {
                    return files[0];
                }
            }
            else if (info.Data.GetDataPresent(ImageCommentDropHandlerProvider.VSProjectItemDataFormat))
            {
                // The drag and drop operation came from the VS solution explorer
                return data.GetText();
            }

            return null;
        }

        /// <summary>
        /// See <see cref="IDropHandler.HandleDragCanceled"/> for more information.
        /// </summary>
        public void HandleDragCanceled()
        {
            //this.manager.PreviewImageAdornment.Clear();
            //this.manager.HighlightLineAdornment.Clear();
        }
    }
}
