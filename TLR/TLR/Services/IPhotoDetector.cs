using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TLR.Enums;

namespace TLR.Services
{
    public interface IPhotoDetector
    {
        Task<Tag> DetectAsync(Stream photo);
    }
}
