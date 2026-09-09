namespace Content.Application.Queries.Volumes.GetStoryVolumes;

public sealed class GetStoryVolumesQueryValidator : AbstractValidator<GetStoryVolumesQuery>
{
    public GetStoryVolumesQueryValidator()
    {
        RuleFor(x => x.StoryId).NotEmpty();
    }
}
