using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace API_QandA.Models
{
    public class QuestionPostRequest
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }
        [Required(ErrorMessage = "Please include some content for the question")]
        public string Content { get; set; }
    }
}
