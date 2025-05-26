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

        public void AddLine(Line line)
        {
            _context.Lines.Add(line);
            _context.SaveChanges();
        }

        public void DeleteLine(Line line)
        {
            _context.Lines.Remove(line);
            _context.SaveChanges();
        }

        public void AddLineShift(LineShift shift)
        {
            _context.LineShifts.Add(shift);
            _context.SaveChanges();
        }

        public void DeleteLineShift(int id)
        {
            var shift = _context.LineShifts.Find(id);
            if (shift != null)
            {
                _context.LineShifts.Remove(shift);
                _context.SaveChanges();
            }
        }

    }
}