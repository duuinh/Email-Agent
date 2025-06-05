using System.ComponentModel.DataAnnotations;

public enum ServiceRequestCategory
{
    [Display(Name = "Account Services")]
    AccountServices,

    [Display(Name = "Card Services")]
    CardServices,

    [Display(Name = "Online/Mobile Banking")]
    OnlineMobileBanking,

    [Display(Name = "Payments & Transfers")]
    PaymentsAndTransfers,

    [Display(Name = "Loan & Mortgage")]
    LoanAndMortgage,

    [Display(Name = "Fraud & Security")]
    FraudAndSecurity,

    [Display(Name = "General Inquiries")]
    GeneralInquiries,

    [Display(Name = "Complaints")]
    Complaints,

    [Display(Name = "Product Information")]
    ProductInformation,

    [Display(Name = "Document Requests")]
    DocumentRequests
}
