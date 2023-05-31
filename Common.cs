using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace Common
{

    public class ImageData
    {
        public ImageData(string imagePath, string label)
        {
            ImagePath = imagePath;
            Label = label;
        }

        public readonly string ImagePath;
        public readonly string Label;
    }

    public class InMemoryImageData
    {
        public InMemoryImageData(byte[] image, string label, string imageFileName)
        {
            Image = image;
            Label = label;
            ImageFileName = imageFileName;
        }

        public readonly byte[] Image;
        public readonly string Label;
        public readonly string ImageFileName;
    }

    public class Images
    {

        public static IEnumerable<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameAsLabel = true)
        {
            var files = Directory.GetFiles(folder, "*", searchOption: SearchOption.AllDirectories);

            foreach (var file in files)
            {
                if ((Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png"))
                    continue;

                var label = Path.GetFileName(file);

                if (useFolderNameAsLabel)
                    label = Directory.GetParent(file).Name;
                else
                {
                    for (int index = 0; index < label.Length; index++)
                    {
                        if (!char.IsLetter(label[index]))
                        {
                            label = label.Substring(0, index);
                            break;
                        }
                    }
                }

                yield return new ImageData(file, label);
            }
        }

    }

}