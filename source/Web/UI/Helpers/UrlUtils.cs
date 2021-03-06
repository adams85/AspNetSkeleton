﻿using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace AspNetSkeleton.UI.Helpers
{
    public static class UrlUtils
    {
        public static StringSegment GetPrefix(PathString path)
        {
            if (!path.HasValue)
                return StringSegment.Empty;

            string pathString = path;
            var index = pathString.IndexOf('/', 1);

            return index >= 0 ? new StringSegment(pathString, 0, index) : new StringSegment(pathString);
        }
    }
}
