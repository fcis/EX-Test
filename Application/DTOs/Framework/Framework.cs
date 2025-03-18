using Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Framework
{
    // DTOs for Framework
    public class FrameworkDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FrameworkStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }
        public List<FrameworkVersionDto> Versions { get; set; } = new List<FrameworkVersionDto>();
    }

    public class FrameworkListDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FrameworkStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime CreationDate { get; set; }
        public int VersionCount { get; set; }
    }

    public class CreateFrameworkDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }

    public class UpdateFrameworkDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public FrameworkStatus Status { get; set; }
    }

    // DTOs for FrameworkVersion
    public class FrameworkVersionDto
    {
        public long Id { get; set; }
        public long FrameworkId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FrameworkVersionStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public DateTime VersionDate { get; set; }
        public DateTime CreationDate { get; set; }
        public long CreatedUser { get; set; }
        public DateTime LastModificationDate { get; set; }
        public long LastModificationUser { get; set; }
    }

    public class CreateFrameworkVersionDto
    {
        [Required]
        public long FrameworkId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime VersionDate { get; set; }
    }

    public class UpdateFrameworkVersionDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public FrameworkVersionStatus Status { get; set; }

        [Required]
        public DateTime VersionDate { get; set; }
    }
}
