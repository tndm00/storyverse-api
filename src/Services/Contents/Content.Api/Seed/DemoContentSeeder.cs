using Content.Domain.Entities;
using Content.Domain.Enums;
using Content.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Content.Api.Seed;

/// <summary>
/// Dev-time content bootstrap. When <c>Seed:DemoContent</c> is true and the
/// Stories table is empty, inserts a small catalogue of Vietnamese horror
/// stories (the "Canh Ba" reader-site content) plus their first chapters, so the
/// reader site and admin console have real rows to render without anyone
/// authoring through the API first.
/// <para>
/// Genres are seeded separately by <see cref="GenreSeeder"/> (which runs first);
/// this seeder looks them up by slug and skips a story whose genre is missing.
/// </para>
/// <para>Idempotent: a no-op once any story exists. Never enable outside dev.</para>
/// </summary>
public sealed class DemoContentSeeder : IHostedService
{
    private const string EnabledKey = "Seed:DemoContent";
    private const long DemoAuthorProfileId = 1;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DemoContentSeeder> _logger;

    public DemoContentSeeder(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DemoContentSeeder> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Runs once at host startup: if enabled and the Stories table is empty,
    /// inserts the demo stories and their chapters. No-op otherwise, and any
    /// failure is logged but never rethrown (must not block app startup).
    /// </summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Feature flag: skip entirely unless demo content seeding is enabled.
        if (!_configuration.GetValue<bool>(EnabledKey))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ContentDbContext>();

        try
        {
            // Idempotency guard: only seed on a truly empty Stories table.
            if (await db.Stories.AnyAsync(cancellationToken))
            {
                return;
            }

            var bySlug = await db.Genres.ToDictionaryAsync(g => g.Slug, cancellationToken);
            var now = DateTime.UtcNow;
            var inserted = 0;

            foreach (var spec in StorySpecs)
            {
                // Skip a story whose genre wasn't seeded (GenreSeeder runs first).
                if (!bySlug.TryGetValue(spec.GenreSlug, out var genre))
                {
                    _logger.LogWarning(
                        "Demo content seed: genre '{Slug}' not found, skipping story '{Title}'.",
                        spec.GenreSlug, spec.Title);
                    continue;
                }

                var publishedAt = now.AddDays(-spec.DaysAgo);

                var story = new Story
                {
                    AuthorProfileId = DemoAuthorProfileId,
                    Title = spec.Title,
                    Slug = spec.Slug,
                    Description = spec.Description,
                    Status = spec.Status,
                    ContentType = StoryContentType.Original,
                    Language = "vi",
                    AgeRating = AgeRating.Mature,
                    ViewCount = spec.Views,
                    RatingAvg = spec.Rating,
                    RatingCount = spec.Views / 20,
                    PublishedAt = publishedAt,
                    CreatedAt = publishedAt,
                    Genres = new List<StoryGenre>
                    {
                        new() { GenreId = genre.Id, IsPrimary = true },
                    },
                };

                // Persist the story first so its generated Id is available for chapters.
                db.Stories.Add(story);
                await db.SaveChangesAsync(cancellationToken);

                for (var i = 0; i < spec.Chapters.Length; i++)
                {
                    var chapterPublishedAt = publishedAt.AddDays(i);

                    db.Chapters.Add(new Chapter
                    {
                        StoryId = story.Id,
                        Title = spec.Chapters[i].Title,
                        OrderIndex = i + 1,
                        Content = spec.Chapters[i].Content,
                        WordCount = spec.Chapters[i].Content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length,
                        Status = ChapterStatus.Published,
                        AccessType = ChapterAccessType.Free,
                        PublishedAt = chapterPublishedAt,
                        CreatedAt = chapterPublishedAt,
                    });
                }

                await db.SaveChangesAsync(cancellationToken);
                inserted++;
            }

            _logger.LogInformation("Demo content seed: inserted {Count} stories.", inserted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Demo content seed failed.");
        }
    }

    /// <summary>No cleanup needed on shutdown.</summary>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private sealed record ChapterSpec(string Title, string Content);

    private sealed record StorySpec(
        string Slug,
        string Title,
        string GenreSlug,
        string Description,
        StoryStatus Status,
        int Views,
        decimal Rating,
        int DaysAgo,
        ChapterSpec[] Chapters);

    private static readonly StorySpec[] StorySpecs =
    {
        new("nguoi-gac-dem-tram-thu-phi-cu", "Người gác đêm ở trạm thu phí cũ", "thanh-thi",
            "Ông Tư nhận ca trực cuối cùng trước khi trạm thu phí đóng cửa vĩnh viễn — nhưng có người vẫn đến trả tiền mỗi đêm.",
            StoryStatus.Ongoing, 2140, 4.6m, 3, new[]
            {
                new ChapterSpec("Ca trực cuối cùng",
                    "Trạm thu phí số 3 đóng cửa từ đầu tháng. Ông Tư được giữ lại thêm một tuần để bàn giao. Đêm đầu tiên, đúng 1 giờ sáng, một chiếc xe khách trờ tới, kính mờ hơi nước. Một bàn tay chìa ra tờ tiền lẻ đã cũ, gấp làm tư. Ông cầm lấy theo thói quen, rồi sực nhớ: barie đã tháo, làn đường đã rào lại từ chiều."),
                new ChapterSpec("Những tờ tiền gấp tư",
                    "Sáng ra, trong ngăn kéo có bảy tờ tiền lẻ, tờ nào cũng gấp làm tư, ẩm và lạnh. Ông Tư hỏi bảo vệ khu công nghiệp gần đó. Người ta bảo mười năm trước có một vụ tai nạn xe khách ngay khúc cua, tài xế cố dừng lại đóng phí thì bị xe sau tông tới."),
            }),
        new("chuyen-xe-cuoi-o-ben-cu", "Chuyến xe cuối ở bến cũ", "thanh-thi",
            "Bến xe liên tỉnh dời đi đã ba năm. Vậy mà 23 giờ 45 mỗi đêm vẫn có một chuyến trả khách ở sân bến cũ.",
            StoryStatus.Completed, 1005, 4.3m, 8, new[]
            {
                new ChapterSpec("Lịch chạy không còn trên bảng",
                    "Tôi làm ca đêm ở nhà kho đối diện bến xe cũ. Bảng giờ chạy đã gỡ, nhà chờ khóa cửa. Nhưng gần nửa đêm, đèn pha một chiếc xe quét qua tường kho, tiếng phanh hơi rít lên, rồi tiếng chân người lục tục bước xuống trên nền xi măng vắng."),
            }),
        new("vi-sao-khong-nen-huyt-sao-ban-dem", "Vì sao không nên huýt sáo ban đêm", "lang-que",
            "Bà tôi có một danh sách những điều không được làm sau khi trời tối. Huýt sáo đứng đầu danh sách đó.",
            StoryStatus.Completed, 742, 4.1m, 12, new[]
            {
                new ChapterSpec("Danh sách của bà",
                    "Không soi gương, không phơi áo, không gọi tên nhau to, và không huýt sáo. Bà không giải thích, chỉ nói: gọi thì phải có người tới. Năm mười hai tuổi, tôi huýt sáo trên đường từ nhà bạn về, một quãng đồng không đèn. Đi được một đoạn, tôi nghe có tiếng huýt sáo đáp lại, đúng điệu tôi vừa thổi, từ phía sau lưng."),
            }),
        new("ba-tieng-go-cua-luc-giao-thua", "Ba tiếng gõ cửa lúc giao thừa", "lang-que",
            "Năm nào cũng vậy, đúng khoảnh khắc chuyển giao có ba tiếng gõ ngoài cửa. Năm nay, bà nội bảo tôi ra mở.",
            StoryStatus.Ongoing, 1412, 4.5m, 5, new[]
            {
                new ChapterSpec("Tục lệ trong nhà",
                    "Nhà tôi có lệ: giao thừa không ai được ra mở cửa, dù có tiếng gõ. Ba tiếng, chậm rãi, cách đều nhau. Bà nội ngồi im trên phản, đếm nhẩm. Năm nay bà đã yếu, bà nắm tay tôi: con lớn rồi, năm nay con ra."),
                new ChapterSpec("Người đứng ngoài hiên",
                    "Tôi mở cửa. Ngoài hiên không có ai, chỉ có đôi dép nhựa cũ đặt ngay ngắn quay mũi vào trong nhà — đôi dép của ông nội, chôn cùng ông đã sáu năm."),
            }),
        new("can-nha-cuoi-hem-khong-so", "Căn nhà cuối hẻm không số", "nha-hoang",
            "Cả con hẻm chỉ có mười hai căn, đánh số từ 1 đến 11. Căn thứ mười hai không có số, và đêm nào đèn cũng sáng.",
            StoryStatus.Ongoing, 3204, 4.7m, 2, new[]
            {
                new ChapterSpec("Căn số mười hai",
                    "Tôi chuyển vào trọ ở căn số 9. Chủ nhà dặn: cuối hẻm có một căn không số, đừng để ý, đừng bấm chuông, và nếu có ai trong đó nhờ giúp gì thì cứ đi thẳng."),
                new ChapterSpec("Người phụ nữ quét sân",
                    "Bốn giờ sáng, tôi thấy một người phụ nữ quét sân trước căn không số. Bà quét rất chậm, cùng một chỗ, tới lui. Tôi chào. Bà ngẩng lên, và tôi nhận ra sân nhà bà không hề có lá."),
                new ChapterSpec("Hồ sơ lưu ở phường",
                    "Anh bạn làm ở phường tra giúp tôi. Con hẻm được quy hoạch mười một căn. Miếng đất cuối hẻm bỏ trống từ 1997 sau một vụ hỏa hoạn. Không có căn thứ mười hai nào trên giấy tờ cả."),
            }),
        new("loi-tat-qua-nghia-trang-lang", "Lối tắt qua nghĩa trang làng", "lang-que",
            "Đi vòng mất bốn mươi phút. Cắt qua nghĩa trang chỉ mười lăm. Đám trẻ chúng tôi luôn chọn lối tắt — cho tới mùa hè năm đó.",
            StoryStatus.Completed, 655, 4.0m, 15, new[]
            {
                new ChapterSpec("Mười lăm phút",
                    "Nghĩa trang làng nằm giữa hai cánh đồng, một lối mòn đất chạy xuyên qua. Ban ngày chẳng ai ngại. Chiều muộn hôm ấy tôi về một mình, đi tới giữa nghĩa trang thì sương xuống nhanh bất thường, và lối mòn phía trước — vốn chỉ có một — bỗng chia làm hai."),
            }),
        new("nguoi-ban-dong-hanh-tren-chuyen-tau-dem", "Người bạn đồng hành trên chuyến tàu đêm", "song-nuoc",
            "Toa số 6, giường 12. Suốt chặng tàu đêm, người khách giường đối diện chỉ ngồi nhìn ra cửa sổ tối đen và không nói một lời.",
            StoryStatus.Ongoing, 1876, 4.4m, 6, new[]
            {
                new ChapterSpec("Giường đối diện",
                    "Tàu rời ga lúc 21 giờ. Khoang bốn giường chỉ có tôi và một người đàn ông gầy, áo sơ mi cũ. Ông không nằm, chỉ ngồi thẳng lưng, nhìn ra ô cửa. Tôi hỏi ông xuống ga nào, ông không quay lại, chỉ nói: 'Ga cuối. Lúc nào cũng ga cuối.'"),
                new ChapterSpec("Danh sách hành khách",
                    "Nhân viên soát vé đi qua hai lần, cả hai lần đều chỉ xé một vé của tôi. Lần thứ ba tôi hỏi thẳng. Anh ta nhìn giường đối diện, rồi nhìn tôi rất lâu, và nói khẽ rằng khoang này bán cho mỗi mình tôi."),
            }),
        new("den-dau-o-gian-tho", "Đèn dầu ở gian thờ không chịu tắt", "lang-que",
            "Ngọn đèn dầu trên bàn thờ nhà ông Bảy đã cháy liên tục bốn mươi chín ngày, không ai châm thêm dầu.",
            StoryStatus.Completed, 531, 3.9m, 20, new[]
            {
                new ChapterSpec("Bốn mươi chín ngày",
                    "Trong tang lễ, người ta thắp một ngọn đèn dầu nhỏ, dặn để cho tới hết 49 ngày. Nhà ông Bảy làm đúng vậy. Chỉ có điều tới ngày thứ mười, bình dầu vẫn đầy nguyên như hôm đầu, mà bấc thì vẫn cháy đều một ngọn lửa xanh."),
            }),
        new("phong-benh-so-13", "Phòng bệnh số 13 không ai dám nhận", "benh-vien",
            "Khoa nội có mười bốn phòng. Phòng 13 luôn để trống, kể cả những đêm bệnh nhân nằm tràn ra hành lang.",
            StoryStatus.Ongoing, 2010, 4.5m, 4, new[]
            {
                new ChapterSpec("Đêm quá tải",
                    "Đêm trực đó bệnh nhân đông chưa từng thấy. Điều dưỡng trưởng cắn răng mở phòng 13, kê hai giường. Chưa đầy một tiếng, cả hai bệnh nhân đều xin chuyển ra hành lang nằm, không ai chịu nói lý do, chỉ lắc đầu."),
                new ChapterSpec("Sổ bàn giao ca",
                    "Tôi lật lại sổ bàn giao mười năm trước. Cứ vài trang lại có một dòng bị gạch xóa nguệch ngoạc, và bên lề, cùng một nét chữ, ghi đúng ba chữ: 'đừng mở 13'."),
            }),
        new("suong-mu-o-deo-ba-tang", "Sương mù ở đèo Ba Tầng", "mien-nui",
            "Cánh tài xế đường dài truyền tai nhau: qua đèo Ba Tầng sau 2 giờ sáng, thấy người vẫy xe thì tuyệt đối không dừng.",
            StoryStatus.Ongoing, 1760, 4.3m, 7, new[]
            {
                new ChapterSpec("Cây số 47",
                    "Tôi chạy chuyến rau đêm, quen đường đèo như lòng bàn tay. Tới cây số 47, đúng khúc cua tay áo, đèn pha rọi vào một người đứng sát mép vực, tay giơ lên vẫy. Tôi đã quen chân đạp phanh — rồi kịp nhớ lời đàn anh dặn, và nhấn ga đi thẳng. Trong gương chiếu hậu, người đó vẫn đứng yên, nhỏ dần, không hạ tay xuống."),
            }),
    };
}
