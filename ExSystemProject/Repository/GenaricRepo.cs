using ExSystemProject.Models;

namespace ExSystemProject.Repository
{
    interface IGenaric<TEntity>
    {
        public List<TEntity> getAll();
        public TEntity getById(int id);
        public void add(TEntity entity);
        public void update(TEntity entity);
        public void delete(int id);
    }
    public class GenaricRepo<TEntity> : IGenaric<TEntity> where TEntity : class
    {
        private readonly ExSystemTestContext _context;

        public GenaricRepo(ExSystemTestContext _context)
        {
            this._context = _context;
        }
        public void add(TEntity entity)
        {
            _context.Set<TEntity>().Add(entity);
        }

        public void delete(int id)
        {
            var x = getById(id);
            //_context.Set<TEntity>().Remove(x);

            if (x != null)
            {
                //x.Isactive = 0;
                var prop = typeof(TEntity).GetProperty("Isactive");
                if (prop != null)
                {
                    prop.SetValue(x, 0);
                    _context.Set<TEntity>().Update(x);
                }
            }
        }

        public List<TEntity> getAll()
        {
            return _context.Set<TEntity>().ToList();
        }

        public TEntity getById(int id)
        {
            return _context.Set<TEntity>().Find(id);
        }

        public void update(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
        }
    }
}
