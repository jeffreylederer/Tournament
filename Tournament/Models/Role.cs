
namespace Tournament.Models
{
    public class Role
    {
        public string RoleText { get; set; }
        public string RoleValue { get; set; }

        public Role(string val, string text)
        {
            RoleText = text;
            RoleValue = val;
        }
    }
}