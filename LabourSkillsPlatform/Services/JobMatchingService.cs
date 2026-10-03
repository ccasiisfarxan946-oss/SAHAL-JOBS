using LabourSkillsPlatform.Data;
using LabourSkillsPlatform.Models;
using Microsoft.EntityFrameworkCore;

namespace LabourSkillsPlatform.Services
{
    public interface IJobMatchingService
    {
        Task<decimal> CalculateMatchScoreAsync(int jobId, string labourerId);
        Task<List<(Labourer Labourer, decimal Score)>> GetRecommendedLabourersAsync(int jobId, int take = 20);
        Task<List<(Job Job, decimal Score)>> GetRecommendedJobsAsync(string labourerId, int take = 20);
    }

    /// <summary>
    /// Implements the multi-criteria matching engine described in Chapter 2 (2.4 - Research Gap #2:
    /// "Context-Aware Semantic Matching") and Chapter 3 (3.4 - Application Tier "Matching Logic").
    ///
    /// The score (0-100) combines:
    ///   - Skill overlap between the Job's required Skills and the Labourer's uploaded Skills (60%)
    ///   - Verified-skill bonus, weighting Admin-verified skills higher than self-declared ones (20%)
    ///   - Labourer's average rating from past Reviews (15%)
    ///   - Location match between the Job and the Labourer (5%)
    ///
    /// This directly addresses the thesis finding that pure GPS/proximity matching is insufficient
    /// (Chapter 2, 2.4) by folding skill-verification and reputation into the score.
    /// </summary>
    public class JobMatchingService : IJobMatchingService
    {
        private readonly ApplicationDbContext _context;

        public JobMatchingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> CalculateMatchScoreAsync(int jobId, string labourerId)
        {
            var job = await _context.Jobs
                .Include(j => j.JobSkills)
                .FirstOrDefaultAsync(j => j.Id == jobId);

            var labourer = await _context.Labourers
                .Include(l => l.LabourerSkills)
                .FirstOrDefaultAsync(l => l.Id == labourerId);

            if (job is null || labourer is null || job.JobSkills.Count == 0)
                return 0;

            var requiredSkillIds = job.JobSkills.Select(js => js.SkillId).ToHashSet();
            var labourerSkills = labourer.LabourerSkills
                .Where(ls => requiredSkillIds.Contains(ls.SkillId))
                .ToList();

            // ----- Skill overlap (60 points max) -----
            var overlapRatio = (decimal)labourerSkills.Count / requiredSkillIds.Count;
            var skillOverlapScore = overlapRatio * 60m;

            // ----- Verified-skill bonus (20 points max) -----
            var verifiedCount = labourerSkills.Count(ls => ls.IsVerified);
            var verifiedRatio = requiredSkillIds.Count == 0 ? 0 : (decimal)verifiedCount / requiredSkillIds.Count;
            var verificationScore = verifiedRatio * 20m;

            // ----- Rating score (15 points max), scaled from a 0-5 rating -----
            var ratingScore = (labourer.AverageRating / 5m) * 15m;

            // ----- Location score (5 points max) -----
            var locationScore = 0m;
            if (!string.IsNullOrWhiteSpace(job.Location) && !string.IsNullOrWhiteSpace(labourer.Location))
            {
                if (string.Equals(job.Location.Trim(), labourer.Location.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    locationScore = 5m;
                }
            }

            var total = skillOverlapScore + verificationScore + ratingScore + locationScore;
            return Math.Round(Math.Min(total, 100m), 2);
        }

        public async Task<List<(Labourer Labourer, decimal Score)>> GetRecommendedLabourersAsync(int jobId, int take = 20)
        {
            var candidateLabourers = await _context.Labourers
                .Include(l => l.User)
                .Include(l => l.LabourerSkills)
                .Where(l => l.AvailabilityStatus == AvailabilityStatus.Available)
                .ToListAsync();

            var results = new List<(Labourer, decimal)>();
            foreach (var labourer in candidateLabourers)
            {
                var score = await CalculateMatchScoreAsync(jobId, labourer.Id);
                if (score > 0)
                {
                    results.Add((labourer, score));
                }
            }

            return results.OrderByDescending(r => r.Item2).Take(take).ToList();
        }

        public async Task<List<(Job Job, decimal Score)>> GetRecommendedJobsAsync(string labourerId, int take = 20)
        {
            var openJobs = await _context.Jobs
                .Include(j => j.Employer).ThenInclude(e => e.User)
                .Include(j => j.Category)
                .Include(j => j.JobSkills).ThenInclude(js => js.Skill)
                .Where(j => j.Status == JobStatus.Open)
                .ToListAsync();

            var results = new List<(Job, decimal)>();
            foreach (var job in openJobs)
            {
                var score = await CalculateMatchScoreAsync(job.Id, labourerId);
                results.Add((job, score));
            }

            return results.OrderByDescending(r => r.Item2).Take(take).ToList();
        }
    }
}
