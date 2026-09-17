namespace Content.Application.Queries.Volumes.GetStoryVolumes;

public sealed class GetStoryVolumesQueryValidator : AbstractValidator<GetStoryVolumesQuery>
{
    /// <summary>Requires a non-empty story id.</summary>
    public GetStoryVolumesQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
