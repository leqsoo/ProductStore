using Rocky.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;

namespace Rocky_DataAccess.Repository
{
    public class InquiryDetailRepository : Repository<InquiryDetail>, IInquiryDetailRepository
    {
        private readonly AplicationDbContext _dbContext;

        public InquiryDetailRepository(AplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(InquiryDetail InquiryDetail)
        {
            _dbContext.Update(InquiryDetail);
        }
    }
}
