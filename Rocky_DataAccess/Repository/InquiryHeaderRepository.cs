using Rocky.Data;
using Rocky_DataAccess.Repository.IRepository;
using Rocky_Models;

namespace Rocky_DataAccess.Repository
{
    public class InquiryHeaderRepository : Repository<InquiryHeader>, IInquiryHeaderRepository
    {
        private readonly AplicationDbContext _dbContext;

        public InquiryHeaderRepository(AplicationDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public void Update(InquiryHeader InquiryHeader)
        {
            _dbContext.Update(InquiryHeader);
        }
    }
}
