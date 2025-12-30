using System;
using System.Collections.Generic;
using System.Text;

namespace PerspectiveRenderer.Config
{
    public static class FixedConfig
    {
        public const int REVOLUTION_SEGMENT_COUNT = 4*36;
        public const float TWO_PI = 2f * (float)Math.PI;
        public const float REVOLUTION_ARC = TWO_PI / REVOLUTION_SEGMENT_COUNT;
    }
}
