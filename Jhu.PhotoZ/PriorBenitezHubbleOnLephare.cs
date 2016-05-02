using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class PriorBenitezHubbleOnLephare : PriorBenitezHubble
    {

        public PriorBenitezHubbleOnLephare(Filter aFilt, Template aTemp) : base(aFilt, aTemp, false)
        {
            if (aTemp is TemplateSimpleLibrary)
            {
                if (((TemplateSimpleLibrary)aTemp).GetNumberOfTemplateSpectra() == 641)
                {
                    DoPreCalculationsFromTemplate(aTemp);
                    return;
                }

            }

            throw new ArgumentException("");
        }

        protected override double ComputeTypeAndRedshiftProb(double aApparentMag, double aRedshift, double aTypeID)
        {
            double probTIfm0 = 0.0;
            double probZIfTAndm0 = 0.0;

            double typeID = Math.Round(aTypeID);
            if (typeID < 8.0)
            {
                probTIfm0 = ComputeEllipticalTypeProb(aApparentMag, 8.0);
                probZIfTAndm0 = ComputeEllipticalRedshiftProb(aApparentMag, aRedshift);
            }
            else if (typeID >= 8.0 && typeID <= 121.0)
            {
                probTIfm0 = ComputeSpiralTypeProb(aApparentMag, 114.0);
                probZIfTAndm0 = ComputeSpiralRedshiftProb(aApparentMag, aRedshift);
            }
            else if (typeID > 121.0 && typeID <= 640.0)
            {
                probTIfm0 = ComputeIrregularTypeProb(aApparentMag, 519.0);
                probZIfTAndm0 = ComputeIrregularRedshiftProb(aApparentMag, aRedshift);
            }

            return probTIfm0 * probZIfTAndm0;
        }


    }
}
