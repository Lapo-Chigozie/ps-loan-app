namespace LapoLoanWebApi.E360Helpers.E360FoundationServices.Model
{
    public class WidgetStaffModel
    {
        public string xParam { get; set; }       // widget.staff.userRef,
        //public string xBuCode { get; set; }
        //public string xScope { get; set; }      // division?['DivisionName'],
        //public string xScopeRef { get; set; }   // division?['DivisionCode'],
        //public int xRowCount { get; set; }

        //public string xFromDate { get; set; }
        //public string xToDate { get; set; }
        //public string xApp { get; set; }
        //public int xPageIndex { get; set; }    //": 1,
        //public int xPageSize { get; set; }     //": 1
    }

    public class WidgetDivisionSearchModel
    {
        public string xParam { get; set; }       // widget.staff.userRef,
        public string xBuCode { get; set; }
        public string xScope { get; set; }      // division?['DivisionName'],
        public string xScopeRef { get; set; }   // division?['DivisionCode'],
        public int xRowCount { get; set; }

        public string xFromDate { get; set; }
        public string xToDate { get; set; }
        public string xApp { get; set; }
        public int xPageIndex { get; set; }    //": 1,
        public int xPageSize { get; set; }     //": 1
    }

    public class WidgetDivisionSearchRespondModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }
        public string Item_Title_Desc { get; set; }
        public string Bu { get; set; }
        public string Gender { get; set; }
        public int xStatusId { get; set; }
        public string xStatus { get; set; }
    }

    public class WidgetDivisionRespondModel
    {
        public string ItemCode { get; set; }
        public string ItemName { get; set; }

        public string FirstName { get; set; }

        public string OtherName { get; set; }

        public string LastName { get; set; }

        public string Item_Title_Desc { get; set; }
        public string Bu { get; set; }
        public string Gender { get; set; }
        public int xStatusId { get; set; }
        public string xStatus { get; set; }
    }

    public class WidgetStaffRespondsModel
    {
        public string data { get; set; }       
    }

    public class WidgetDivisionModel
    {
        public string tk { get; set; }       // widget.staff.userRef,
        public string us { get; set; }
        public string rl { get; set; }      // division?['DivisionName'],
        public string src { get; set; }   // division?['DivisionCode'],

        // 'tk': widget.auth?.token,
        // 'us': widget.staff.userRef,
        // 'rl': widget.staff.uRole,
        // 'src': "AS-IN-D659B-e3M"
    }
}
