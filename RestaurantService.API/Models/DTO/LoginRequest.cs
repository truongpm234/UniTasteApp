namespace zPayment.MVCWebApp.FE.VuLNS.Models
{
    public class LoginRequest
    {
        /*
         UserName = {userName, email, phone, employeeCode}
         */
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
