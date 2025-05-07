using Optimisation_and_Scheduling_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Optimisation_and_Scheduling_System.Repositories.Interfaces
{
    public interface ILineRepository
    {
        List<Line> GetAllLines();
        Line GetLineById(int id);
        List<LineShift> GetLineShifts(int lineId);

        void AddLine(Line line);  // Method to add a line
        void DeleteLine(Line line); // Method to delete a line

        void AddLineShift(LineShift shift);
        void DeleteLineShift(int id);

    }
}