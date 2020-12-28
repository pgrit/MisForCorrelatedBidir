using System;
using System.Collections.Generic;

namespace MisForCorrelatedBidir.Common {
    public abstract class RadiusInitializer {
        public abstract float ComputeRadius(float sceneRadius, float primaryPdf, float primaryDistance);
    }

    public class RadiusInitScene : RadiusInitializer {
        public float ScalingFactor;
        public override float ComputeRadius(float sceneRadius, float primaryPdf, float primaryDistance)
        => sceneRadius * ScalingFactor;
    }

    public class RadiusInitPixel : RadiusInitializer {
        public float ScalingFactor;
        public override float ComputeRadius(float sceneRadius, float primaryPdf, float primaryDistance)
        => ScalingFactor / MathF.Sqrt(primaryPdf * MathF.PI);
    }

    public class RadiusInitFov : RadiusInitializer {
        public float ScalingFactor;
        public override float ComputeRadius(float sceneRadius, float primaryPdf, float primaryDistance)
        => primaryDistance * ScalingFactor;
    }

    public class RadiusInitCombined : RadiusInitializer {
        public List<RadiusInitializer> Candidates;
        public override float ComputeRadius(float sceneRadius, float primaryPdf, float primaryDistance) {
            float minRadius = float.MaxValue;
            foreach (var c in Candidates) {
                float r = c.ComputeRadius(sceneRadius, primaryPdf, primaryDistance);
                minRadius = MathF.Min(minRadius, r);
            }
            return minRadius;
        }
    }
}