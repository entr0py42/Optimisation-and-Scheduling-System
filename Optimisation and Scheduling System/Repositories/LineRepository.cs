using Optimisation_and_Scheduling_System.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Optimisation_and_Scheduling_System.Repositories.Interfaces;

namespace Optimisation_and_Scheduling_System.Repositories
{
    public class LineRepository : ILineRepository
    {
        private readonly AppDbContext _context;

        public LineRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Line> GetAllLines()
        {
            return _context.Lines.ToList();
        }

        public Line GetLineById(int id)
        {
            return _context.Lines.FirstOrDefault(line => line.Id == id);
        }

        public List<LineShift> GetLineShifts(int lineId)
        {
            return _context.LineShifts.Where(ls => ls.LineId == lineId).ToList();
        }
    }
}