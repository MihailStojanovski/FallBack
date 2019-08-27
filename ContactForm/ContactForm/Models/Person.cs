using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ContactForm.Models
{
    public class Person
    {
        [Key]
        [Required]
        public int PersonId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        [StringLength(20, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string LastName { get; set; }
        
        [Required]
        [EmailAddress]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Invalid")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        [Display(Name = "Comment")]
        public string Comment { get; set; }


    }

    public class PersonDBContext : DbContext
    {
        public PersonDBContext(): base(@"Data Source=MIHAIL-PC\SQL2008R2;Initial Catalog=ContactFormDb;Integrated Security=True") 
        {
            Database.SetInitializer<PersonDBContext>(null);

        }
        public DbSet<Person> Persons { get; set; }
    }
}