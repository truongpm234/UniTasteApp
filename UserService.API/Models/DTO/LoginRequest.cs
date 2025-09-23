namespace zPayment.MVCWebApp.FE.VuLNS.Models
{
    public class LoginRequest
    {
        /*
         UserName = {userName, email, phone, employeeCode}
         */
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
