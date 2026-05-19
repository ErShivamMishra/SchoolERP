using SchoolERP.Application.Features.Study.Models;

namespace SchoolERP.Application.Features.Study.Interfaces;

public interface IStudyService
{
    Task<SubjectDto> CreateSubjectAsync(CreateSubjectRequestDto request, CancellationToken cancellationToken);
    Task<SubjectDto> UpdateSubjectAsync(Guid subjectId, UpdateSubjectRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<SubjectDto>> GetSubjectsAsync(Guid? schoolId, CancellationToken cancellationToken);
    Task<SyllabusDto> UploadSyllabusAsync(UploadSyllabusRequestDto request, CancellationToken cancellationToken);
    Task<StudyMaterialDto> UploadStudyMaterialAsync(UploadStudyMaterialRequestDto request, CancellationToken cancellationToken);
    Task<HomeworkAssignmentDto> CreateHomeworkAsync(CreateHomeworkAssignmentRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<StudyMaterialDto>> GetStudyMaterialsAsync(GetStudyMaterialsRequestDto request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<HomeworkAssignmentDto>> GetHomeworkAssignmentsAsync(GetHomeworkAssignmentsRequestDto request, CancellationToken cancellationToken);
}
