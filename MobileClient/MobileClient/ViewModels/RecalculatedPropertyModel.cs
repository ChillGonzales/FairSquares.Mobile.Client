using FairSquares.Measurement.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MobileClient.ViewModels
{
    public class RecalculatedPropertyModel : PropertyModel
    {
        public RecalculatedPropertyModel(PropertyModel original)
        {
            this.Name = original.Name;
            this.OrderId = original.OrderId;
            this.PropertyId = original.PropertyId;
            this.Description = original.Description;
            this.Address = original.Address;
            this.ReportType = original.ReportType;
            this.Roofs = original.Roofs;
        }
        public int OriginalPitch { get; set; }
        public int CurrentPitch { get; set; }
        public List<SectionModel> RecalculatedSections { get; set; }
    }
}
