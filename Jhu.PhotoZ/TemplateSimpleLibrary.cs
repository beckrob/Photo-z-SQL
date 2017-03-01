using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jhu.PhotoZ
{
    public class TemplateSimpleLibrary : Template
    {
        private List<Spectrum> templateList;

        public TemplateSimpleLibrary(List<Spectrum> aTemplates, TemplateParameter aRedshift, TemplateParameter aLuminosity = null)
            : base(aRedshift, aLuminosity)
        {
            if (!ReferenceEquals(aTemplates, null))
            {
                templateList = aTemplates;
            } 
            else 
            {
                templateList = new List<Spectrum>();
            }
            parameterList.Add(new TemplateParameterAdditiveRandomIterator(0, templateList.Count - 1, 1) { Name = "TypeID" });
            //parameterList.Add(new TemplateParameterAdditive(0, templateList.Count - 1, 1) { Name = "TypeID", Value = 0});
        }

        protected TemplateSimpleLibrary() {}

        //Does not create a copy of the templates, just parameters and simple values
        public override Template CloneLightWeight()
        {
            TemplateSimpleLibrary copy = new TemplateSimpleLibrary();

            CloneLightWeighValues(copy); 

            return copy;
        }

        protected void CloneLightWeighValues(TemplateSimpleLibrary other)
        {
            other.templateList = templateList;
            base.CloneLightWeighValues(other);
        }

        public int GetNumberOfTemplateSpectra()
        {
            return templateList.Count;
        }

        public void AddTemplateSpectrum(Spectrum aTemplate)
        { 
            templateList.Add(aTemplate);
            parameterList[2] = new TemplateParameterAdditiveRandomIterator(0, templateList.Count - 1, 1) { Name = "TypeID" };
            //double val = parameterList[2].Value;
            //parameterList[2] = new TemplateParameterAdditive(0, templateList.Count - 1, 1) { Name = "TypeID", Value = val };
        }

        public void RemoveTemplateSpectrum(int atemplateID)
        {
            templateList.RemoveAt(atemplateID);
            parameterList[2] = new TemplateParameterAdditiveRandomIterator(0, templateList.Count - 1, 1) { Name = "TypeID" };
            /*double val=parameterList[2].Value;
            if (val >= atemplateID && val > 0)
            {
                --val;
            }
            parameterList[2] = new TemplateParameterAdditive(0, templateList.Count - 1, 1) { Name = "TypeID", Value = val };*/
        }

        public void RemoveAllTemplateSpectra()
        {
            templateList.Clear();
            parameterList[2] = new TemplateParameterAdditiveRandomIterator(0, templateList.Count - 1, 1) { Name = "TypeID" };
            //parameterList[2] = new TemplateParameterAdditive(0, templateList.Count - 1, 1) { Name = "TypeID", Value = 0 };
        }

        public override Spectrum GenerateSpectrum()
        {
            Spectrum result = new Spectrum(templateList[(int) Math.Round(parameterList[2].Value)]);

            result.Luminosity = parameterList[0].Value;
            result.Redshift = parameterList[1].Value;

            return result;
        }

    }
}
