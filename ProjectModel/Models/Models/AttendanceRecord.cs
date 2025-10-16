
using Google.Cloud.Firestore;
using Models.Utils;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Reflection;

namespace Models
{
    public static class ProjectConsts
    {
        public const string Visitors = "visitors";
        public const string Questions = "ScreeningCheckQuestions";
        public const string Users = "users";
        public const string Attendance = "attendanceRecords";
        public const string VisitorsAnswers = "answers";
    }

    public class AttendanceWithVisitor
    {
        public AttendanceRecord Record { get; set; } = new AttendanceRecord();
        public Visitor Visitor { get; set; } = new Visitor();
        public bool Expanded { get; set; } = false;

        public string CheckOutText()
        {
            if (Record.CheckOutTime.HasValue)
            {
                return Record.CheckInTime.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
            }
            return Record.CheckInTime.Date.ToLocalTime() == DateTime.UtcNow.Date ? "On Site" : "Never checked out";
        }
    }

    [FirestoreData]
    public class AttendanceRecord
    {
        // Firestore document ID will be stored here once retrieved
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string VisitorID { get; set; } = string.Empty;

        [FirestoreProperty]
        public DateTime CheckInTime { get; set; }

        // The time when the visitor permanently checks out (nullable if not yet checked out)
        [FirestoreProperty]
        public DateTime? CheckOutTime { get; set; }

        // Indicates if the visitor was pre-registered
        [FirestoreProperty]
        public bool IsPreRegistered { get; set; }

        [FirestoreProperty]
        public string? VehicleReg { get; set; }

        // A collection of sessions for this visitor (each session records a check-in and check-out)
        [FirestoreProperty]
        public List<VisitorSession> Sessions { get; set; } = new List<VisitorSession>();

        // Indicates if this session is finalized (visitor permanently left for this visit)
        [FirestoreProperty]
        public bool PermanentlyLeft { get; set; } = false;

        [FirestoreProperty]
        public string? SignatureImageData { get; set; }

        [FirestoreProperty]
        public string? UserSignatureImageData { get; set; }
    }

    [FirestoreData]
    public class VisitorSession
    {
        // The time when the visitor checks in
        [FirestoreProperty]
        public DateTime CheckInTime { get; set; }

        [FirestoreProperty]
        // The time when the visitor checks out (nullable if not yet checked out)
        public DateTime? CheckOutTime { get; set; }
    }


    [FirestoreData]
    public class Visitor
    {
        // Firestore document ID will be stored here once retrieved
        [FirestoreDocumentId]
        public string Id { get; set; } = string.Empty;

        [FirestoreProperty]
        public VisitorDetails Details { get; set; } = new VisitorDetails();

        [FirestoreProperty]
        public DateTime? CreatedAt { get; set; }

        [FirestoreProperty]
        public DateTime? LastVisit { get; set; }

        public string GetLastVisit()
        {

            if (LastVisit == null)
                return "No previous visits";

            var lastVisitDate = LastVisit.Value.ToLocalTime();
            return lastVisitDate.ToString("dd/MM/yyyy HH:mm");
        }
    }


    [FirestoreData]
    public class VisitorDetails
    {

        [FirestoreProperty]
        public string VisitorName { get; set; } = string.Empty;

        [FirestoreProperty]
        public string? CompanyName { get; set; }

        [FirestoreProperty]
        public string? MobileNumber { get; set; }

        [FirestoreProperty]
        public VisitorType Type { get; set; }

        [FirestoreProperty]
        public bool HighVisIssued { get; set; }

        [FirestoreProperty]
        [JsonConverter(typeof(YesNoNullEnumConverter))]
        public NA_Yes_No HighVisReturned { get; set; } = NA_Yes_No.NA;

        [FirestoreProperty]
        public NA_Yes_No? Approved { get; set; }

        [FirestoreProperty]
		public DateTime? QuestionerExpiryDate { get; set; }
	}

    public enum VisitorType
    {
        [Display(Name = "Visitor")]
        Visitor,
        
        [Display(Name = "Contractor")]
        Contractor,
        
        [Display(Name = "Haulage")]
        Haulage,
        
        [Display(Name = "Office Visitor")]
        OfficeVisitor,
        
        [Display(Name = "Vehicle")]
        Vehicle,
    }

    public static class VisitorTypeExtensions
    {
        public static string ToDisplayString(this VisitorType visitorType)
        {
            var displayAttribute = visitorType.GetType()
                .GetField(visitorType.ToString())
                ?.GetCustomAttributes(typeof(DisplayAttribute), false)
                .FirstOrDefault() as DisplayAttribute;

            return displayAttribute?.Name ?? visitorType.ToString();
        }
    }

    [JsonConverter(typeof(YesNoNullEnumConverter))]
    public enum NA_Yes_No
    {
        NA,
        Yes,
        No
    }

    public class DateOnlyRequest
    {
        [JsonConverter(typeof(DateTimeConverterUsingDateTimeParse))]
        public DateTime Date { get; set; }
    }

    public class CheckoutWithCustomTimeRequest
    {
        [Required]
        public DateTime CheckoutTime { get; set; }

        [Required]
        public bool IsPermanent { get; set; }
    }

    [FirestoreData]
    public class  QuestionerData
    {
        // Firestore document ID will be stored here once retrieved
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string VisitorID { get; set; } = string.Empty;

        [FirestoreProperty]
        public System.DateTime? CreatedOn { get; set; } = System.DateTime.UtcNow;

        [FirestoreProperty]
        public List<Answers> Answers { get; set; } = new();

        [FirestoreProperty]
        public string? SignatureImageData { get; set; }

        [FirestoreProperty]
        public string? UserSignatureImageData { get; set; }

        [FirestoreProperty]
        public bool AcceptedTerms { get; set; } = false;

    }

    [FirestoreData]
    public class Answers
    {

        [FirestoreProperty]
        public string QuestionId { get; set; } = string.Empty;

        [FirestoreProperty]
        public List<string> AnswersList { get; set; } = new List<string>();
    }

    [FirestoreData]
    public class Questioner
    {
        [FirestoreDocumentId]
        public string? Id { get; set; }

        [FirestoreProperty]
        public string Question { get; set; } = string.Empty;

        [FirestoreProperty]
        public bool IsRequired { get; set; } = true;

        [FirestoreProperty]
        public bool IsRequiredExtraAnswer { get; set; } = false;

        [FirestoreProperty]
        public string ExtraQuestion { get; set; } = string.Empty;
        
        [FirestoreProperty]
		public bool ShouldBeFalse { get; set; } = true;
        
        [FirestoreProperty]
		public bool CanBeTrueOrFalse { get; set; } = false;  
        
        [FirestoreProperty]
		public string Category { get; set; } = string.Empty;
    }

    public class VisitorWithQuestionerData
    {
        public Visitor Visitor { get; set; } = new Visitor();
        public QuestionerData? QuestionerData => AllQuestionerData?.FirstOrDefault();
        public List<QuestionerData>? AllQuestionerData { get; set; }

        public bool ShowDetails { get; set; } = false;

        public VisitorWithQuestionerData(Visitor InVisitor, List<QuestionerData> questionerData)
        {
			Visitor = InVisitor;
            AllQuestionerData = questionerData;

		}

        public VisitorWithQuestionerData(Visitor InVisitor, QuestionerData questionerData )
		{
            Visitor = InVisitor;
			AllQuestionerData = new List<QuestionerData>
			{
				questionerData
			};
		}

        public string GetQuestionerDataCreatedOnString()
        {
            if (AllQuestionerData == null || AllQuestionerData.Count == 0)
                return "N/A";

            var createdOn = AllQuestionerData[0].CreatedOn;
            return createdOn != null ? createdOn.Value.ToLocalTime().ToShortDateString() : "N/A";
        }

        public bool Expired()
        {
            var result = Visitor.Details.QuestionerExpiryDate < DateTime.UtcNow;
			return result;
        }
    }

    public class AllQuestions
    {
        public List<Questioner> Questioners { get; set; } = new List<Questioner>();

        public AllQuestions()
        {
            // Have you now, or the last seven days suffered from diarrhoea and/ or vomiting?	
            Questioner question1 = new Questioner
            {
                Id = "aK4RVKdurz9Ls464Zndb",
                Question = "Have you now, or the last seven days suffered from diarrhoea and/ or vomiting?",
                Category = "Health",
            };

            //At Present are you suffering from: Skin trouble affecting hands, arms or face ?
            Questioner question2 = new Questioner
            {
                Id = "MISvRNoVL0zaLhPaQ6by",
                Question = "At Present are you suffering from: Skin trouble affecting hands, arms or face ?",
                Category = "Health",
            };

            //At Present are you suffering from: Boils, styes or septic fingers?
            Questioner question3 = new Questioner
            {
                Id = "UZf8bJbv2lMZ68fqd3zJ",
                Question = "At Present are you suffering from: Boils, styes or septic fingers?",
                Category = "Health",
            };

            //At Present are you suffering from: Discharge from eye, ear, gums or mouth?
            Questioner question4 = new Questioner
            {
                Id = "UsWwADhRMp1HHMi9VdIQ",
                Question = "At Present are you suffering from: Discharge from eye, ear, gums or mouth?",
                Category = "Health",
            };

            //Do you suffer from: Recurring skin or ear trouble? 
            Questioner question5 = new Questioner
            {
                Id = "bjRP0OWUn1c74KFvOk9B",
                Question = "Do you suffer from: Recurring skin or ear trouble?",
                Category = "Health",
            };

            //Do you suffer from: Recurring bowel disorder? 
            Questioner question6 = new Questioner
            {
                Id = "52XaUsJmpvc0LZjRAE8F",
                Question = "Do you suffer from: Recurring bowel disorder?",
                Category = "Health",
            };

            //Have you ever had or are you known to be a carrier of typhoid or paratyphoid?	
            Questioner question7 = new Questioner
            {
                Id = "jGmDiD8xO1K9wIED4BDi",
                Question = "Have you ever had or are you known to be a carrier of typhoid or paratyphoid?",
                Category = "Health",
            };

            //In the last 21 days have you be in contact with anyone, at home or abroad, who may have been being suffering from typhoid or paratyphoid?  
            Questioner question8 = new Questioner
            {
                Id = "PcXRvtGVxSE24FQ0Ymog",
                Question = "In the last 21 days have you be in contact with anyone, at home or abroad, who may have been being suffering from typhoid or paratyphoid?",
                Category = "Health",
            };

            //Have you been abroad in the last month ?
            Questioner question9 = new Questioner
            {
                Id = "bbKZQB4Yd90xFT072yMP",
                Question = "Have you been abroad in the last month?",
                Category = "Travel",
                IsRequiredExtraAnswer = true,
                ExtraQuestion = "If yes, what country?",
				CanBeTrueOrFalse = true
			};

            //Have you removed all jewellery (watches, bracelets, earrings etc.) before entering manufacturing areas? 
            Questioner question10 = new Questioner
            {
                Id = "OWx7WBEfWKmWrIDs8nw5",
                Question = "Have you removed all jewellery (watches, bracelets, earrings etc.) before entering manufacturing areas?",
                ShouldBeFalse = false,
                Category = "Food safety",
            };

            //Please be aware that this is a nut free site. Tick to confirm you understand?
            Questioner question11 = new Questioner
            {
                Id = "o7EBp8niMcLnPtWHtivY",
                Question = "Please be aware that this is a nut free site. Tick to confirm you understand",
				ShouldBeFalse = false,
                Category = "Food safety",
            };
            
            //Please be aware that this is a nut free site. Tick to confirm you understand?
            Questioner question12 = new Questioner
            {
                Id = "1Aqd1dY1P8wIHUhfJE9n",
                Question = "Use of cameras or electronic devices on the premises is strictly forbidden. Do you understand and agree to comply with this policy?",
				ShouldBeFalse = false,
                Category = "Site Security",
            };


/*            Questioner question13 = new Questioner
            {
                Question = "Have you removed all jewellery (watches, bracelets, earrings etc.) before entering manufacturing areas, except for a plain wedding band?",
                ShouldBeFalse = false,
                Category = "Food safety",
            };*/

            Questioner question14 = new Questioner
            {
                Id = "wNFJqeKFOHR1MfB31Ucx",
                Question = "Are your fingernails clean, short, and free of nail varnish?",
                ShouldBeFalse = false,
                Category = "Food safety",
            };

            Questioner question15 = new Questioner
            {
                Id = "lq7oMCKrsL8xzYtsILGo",
                Question = "Can you confirm you are not wearing false eyelashes?",
                ShouldBeFalse = false,
                Category = "Food safety",
            };

            Questioner question16 = new Questioner
            {
                Id= "SPLkujRYqUqhCac97dtb",
                Question = "Can you confirm you are not wearing excessive perfume, deodorant, or aftershave?",
                ShouldBeFalse = false,
                Category = "Food safety",
            };

            Questioner question17 = new Questioner
            {
                Id = "n0rYvXjdeV4vi9eWRe5w",
                Question = "Are all cuts on your body covered with metal detectable plasters?",
                ShouldBeFalse = false,
                Category = "Food safety",
            };


            Questioner question18 = new Questioner
            {
                Id = "CceZqaI4ajqAQ7B2FLOO",
                Question = "Are you suffering from fever, chills, cough, and shortness of breath, fatigue, aches , pains or High Temperature?",
                ShouldBeFalse = true,
                Category = "Covid 19",
            };

            Questioner question19 = new Questioner
            {
                Id = "y6xNuyqb9Suli1C6Iwhh",
                Question = "Are you a household contact or a close contact of anyone who has had covid-19 symptoms?",
                ShouldBeFalse = true,
                Category = "Covid 19",
            };



            //1. Are you suffering from fever, chills, cough, and shortness of breath, fatigue, aches , pains or High Temperature?
            //3. Are you a household contact or a close contact of anyone who has had covid-19 symptoms?

            Questioners.Add(question1);
            Questioners.Add(question2);
            Questioners.Add(question3);
            Questioners.Add(question4);
            Questioners.Add(question5);
            Questioners.Add(question6);
            Questioners.Add(question7);
            Questioners.Add(question8);
            Questioners.Add(question9);
            Questioners.Add(question10);
            Questioners.Add(question11);
            Questioners.Add(question12);
            //Questioners.Add(question13);
            Questioners.Add(question14);
            Questioners.Add(question15);
            Questioners.Add(question16);
            Questioners.Add(question17);
            Questioners.Add(question18);
            Questioners.Add(question19);
        }
    }
}
