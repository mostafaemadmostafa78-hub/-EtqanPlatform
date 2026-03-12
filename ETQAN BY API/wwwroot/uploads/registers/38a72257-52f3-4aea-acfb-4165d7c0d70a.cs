using BookStore.Data;

namespace BookStore.UnitOfWork
{
    public class UnitOFWork
    {
        /*
         create context
         begin transaction
                    commit
                    rollback
          save
         */
        BookStoreContext context;
        //reposioty
        public UnitOFWork(BookStoreContext _Context)
        {
            context = _Context;
            BeginTransaction();
        }
        public void BeginTransaction()
        {
            if(context.Database.CurrentTransaction is null)
            {
                context.Database.BeginTransaction();
            }
        }
        //save or rollback
        public void Save()
        {
            try
            {
                context.SaveChanges();
            }catch (Exception ex) {
                context.Database.CurrentTransaction.Rollback();
            }
        }

        public void SaveAndCommit()
        {
            try
            {
                context.SaveChanges();
                //commit
                context.Database.CurrentTransaction.Commit();
            }
            catch (Exception ex)
            {
                context.Database.CurrentTransaction.Rollback();
            }
        }
    }
}
