using Payroll.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payroll.ViewModels
{

    public class Coverage
    {
        public static _Employee[] Employees;
        public static _WorkTime[] WorkTime =
            new[] { new _WorkTime(00, 1, "M", ""), new _WorkTime(1, 2, "E", ""), new _WorkTime(2, 3, "N", "") };
        public static _Day[] Days;


        // Coverage for each sibjects
        public static double[] PercentCoverages = { .61, .31, .39, .2 };
        public static int[] CoverageWeights = { 5, 2, 8, 1 };
        public static int MaxWeight = (5 + 2 + 8 + 1);

        public static double WeightCoverageTotal = .75;
        public static double WeightCoverageAvg = .25;

        public static double WeightCoverage = .01;
        public static double WeightDistribution = 1000;
        public static double WeightRequestedSlotsStaffed = 1;
        public static double WeightContinousBlocks = 1;

        public static int[] CoverageWeightCounts;

        public static int TOTAL_DAYS = 7;
        internal static int TOTAL_SLOTS = TOTAL_DAYS * WorkTime.Count();


        public static void CalcCoverageWeightCounts()
        {
            CoverageWeightCounts = new int[MaxWeight + 1];

            //calc employee weights
            for (int i = 1; i < CoverageWeightCounts.Length; i++)
            {
                for (int j = 0; j < Coverage.Employees.Length; j++)
                {
                    if (i == Coverage.Employees[j].CalcCoverageWeight())
                        CoverageWeightCounts[i]++;
                }
            }
        }
    }


    public class _Day
    {
        private readonly int day;

        public _Day(int day)
        {
            this.day = day;
        }

        public int GetDay() { return day; }
    }

    [Serializable]
    public class _Work
    {
        public string Name { get; set; }
        public string Color { get; set; }
        public int id { get; set; }
        public WorkType Type { get; set; }
        
    }

    [Serializable]
    public class _WorkTime
    {
        public string Name { get; set; }

        public string Color { get; set; }
        public int id { get; set; }
        internal int index;
        public bool IsSelected { get; set; }
        public int MinEmployees { get; set; }
        public int MaxEmployees { get; set; }

        public IList<WorkTimeWorkItem> WorkTimeWorkItems { get; set; }

        public _WorkTime()
        {

        }
        public _WorkTime(int index, int id, string name, string color)
        {
            this.index = index;
            this.id = id;
            Name = name;
            Color = color;
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }

    public class WorkTimeWorkItem
    {
        public string guid => WorkId + WorkTimeId + IsForWholeWeek.ToString();
        public int WorkTimeId { get; set; }
        public int WorkId { get; set; }
        public bool IsForWholeWeek { get; set; }
        public IList<DayOfWeek> OnDays { get; set; }

        public int[] WorkableEmployeeIds { get; set; }
        public int MinEmployees { get; set; }
        public int MaxEmployees { get; set; }
        //public WorkTimeWorkItem()
        //{
        //    guid = Guid.NewGuid();
        //}
    }

    [Serializable]
    public class _Employee
    {
        public int Index = 0;
        public int id;
        public string Name;
        public string Photo;
        public string Department;

        // coverage for each employee skills
        public bool[] CoverageMatrix = { false, false, false, false };
        public bool[] RequestedSlots = new bool[Coverage.TOTAL_SLOTS];
        public bool[] BlackOutSlots = new bool[Coverage.TOTAL_SLOTS];

        public bool[] NonWorkingShifts = new bool[Coverage.WorkTime.Length];

        private Random rand = new Random();
        private int i;

        public _Employee()
        {

        }
        public _Employee(int index, string name)
        {
            Index = index;
            Name = name;

            int weight = 0;

            //randomly assign this employee coverage subjects based on expected percentages
            for (int coverage = 0; coverage < 4; coverage++)
            {
                if (rand.NextDouble() <= Coverage.PercentCoverages[coverage])
                    this.CoverageMatrix[coverage] = true;
            }

            weight = this.CalcCoverageWeight();

            if (weight == 0)
                weight = 1;

            //regenerate a 0 weighted employee (everyone must cover something)
            if (weight == 0)
                throw new Exception("weigt is 0;");
            //var _ = new Employee(index, name);
            else
            {
                int start;
                int maxAdd;

                //randomly assign this employee 20-40 desired slots
                int maxSlots = rand.Next(20, 40);

                for (int slot = 0; slot < maxSlots; slot++)
                {
                    //pick random starting point
                    start = rand.Next(0, Coverage.TOTAL_SLOTS - 1);

                    //pick random amount between 1 and 10 to add
                    maxAdd = rand.Next(1, 10);
                    for (int add = 0; add < maxAdd; add++)
                    {
                        this.RequestedSlots[start + add] = true;
                        slot++;

                        if (start + add >= Coverage.TOTAL_SLOTS - 1)
                        {
                            //reset back to 0 slot
                            maxAdd = maxAdd - add;
                            add = -1;
                            start = 0;
                        }
                    }
                }

            }
        }

        public _Employee(int i, int id, string nameDisplay, string photoLink, string department)
        {
            this.i = i;
            this.id = id;
            this.Name = nameDisplay;
            this.Photo = photoLink;
            this.Department = department;
        }

        public Gender Gender { get;  set; }
        public string Designation { get;  set; }

        // pointers where working (rules)
        // set in step 4 - set shift weights and assign TASKS
        public string[] ShiftWorkPointers { get; internal set; }
        public int[] WorkableTaskIds { get; internal set; }
        public int _PatternStartIndex { get;  set; }
        public int _PatternEndIndex { get;  set; }
        //public string ContractName { get;  set; }
        //public string ContractDuration { get;  set; }
        //public int ContractId { get;  set; }
        //public string ContractDesignation { get;  set; }
        //public bool ContractActive { get;  set; }
        //public DateTime ContractStart { get;  set; }
        //public DateTime ContractEnd { get;  set; }

        public int CalcCoverageWeight()
        {
            int ret = 0;
            for (int i = 0; i < 4; i++)
            {
                ret += ((CoverageMatrix[i]) ? Coverage.CoverageWeights[i] : 0);
            }
            return ret;
        }

        public void InitializeSlotArrays(int slotCount)
        {
            BlackOutSlots = new bool[slotCount];
            RequestedSlots = new bool[slotCount];
        }
    }



    [Serializable]
    public class DayShiftSlot
    {
        // a random number between each min and max value is then
        // chosen for each day-hour slot when the GA
        // population is created.This functionality is
        // intended to provide an administrator minimum
        // and maximum values to control staffing expenses.
        public int MinEmployees;
        public int MaxEmployees;
        public int SelectedEmployees = 0;
        public int index;


        public _Employee[] Employees;
        private _WorkTime WorkTime;
        public int Day;

        public DayShiftSlot(int day)
        {
            Day = day;
        }

        public double CalcFitness()
        {
            double totalFitness = 0;

            //% subject coverage (same as maximizing avg. coverage weight and total coverage weight)
            for (int i = 0; i < SelectedEmployees; i++)
            {
                if (Employees[i] != null)
                    totalFitness += Employees[i].CalcCoverageWeight();
            }

            return (totalFitness * Coverage.WeightCoverageTotal)
                +
                ((SelectedEmployees == 0) ? 0 :
                (totalFitness / SelectedEmployees * Coverage.WeightCoverageAvg));
        }



        public override string ToString()
        {
            return Day + WorkTime.Name.ToString() + "[" + string.Join(", ", Employees.Select(x => x.Name)) + "]";
        }

        internal void SetShift(_WorkTime workTime)
        {
            this.WorkTime = workTime;
        }
        internal _WorkTime GetShift()
        {
            return this.WorkTime;
        }

        internal void SetDay(int day)
        {
            this.Day = day;
        }

        internal void SetMinMaximumEmployees(int length)
        {
            MinEmployees = 3;
            MaxEmployees = 8;
        }

        public int GetDay() => this.Day;
    }


    public class RosterVm
    {
        public object NextChildScheduleName { get; private set; }
        public int ScheduleId { get; set; }
        public RosterCreateLineItemType SelectedMenu { get; set; }
        public int? ParentScheduleId { get; private set; }

        public string SelectedMenuString => $"Step {((int)SelectedMenu)} · {Header}";
        public List<RosterCreateMenu> RoterCreateMenu { get;  set; }
        public int[] EmployeeIds { get; set; }
        public int[] WorkTimeIds { get; set; }
        
        public List<_Employee> Employees { get; set; }
        public List<_WorkTime> WorkTimes { get;  set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        //public string Duration => StartDate.GetDuration(EndDate ?? DateTime.MaxValue);
        public int Slots { get; set; }
        public string Name { get; set;  }
        public string Header { get;  set; }
        public List<Schedule> RecentSchedules { get;  set; }
        public List<DayVm> DaysData { get;  set; }
        public List<Work> Works { get;  set; }
        public string _PattenString { get;  set; }
        public char[] _Patten { get;  set; }
        public double _TotalWorkingHoursPerWeek { get;  set; }
        public int _ConseqetiveDays { get;  set; }
        public List<WeeklyEmployeeShiftVm> WeekEmployeeShift { get;  set; }
        public DayOfWeek CompanyWorkStartDay { get;  set; }
        //public bool IsRoseterGenerated { get; internal set; }
        public List<WorkItemScheduleVvm> WeekEmployeeTask { get;  set; }
        //public List<EmployeeInteraction> ScheduleInteractions { get; internal set; }
        public DateTime RosterGeneratedDate { get;  set; }
        public object ParentScheduleName { get; }
        public int? NextChildScheduleId { get; private set; }
        //public bool IsPublished { get; internal set; }
        public ScheduleStatus Status { get;  set; }
        public bool EmployeeActiveContractFlag { get;  set; }
        public List<Notification> Notifications { get;  set; }
        public List<EmployeeType> ActiveContracts { get;  set; }

        public void CalculateSlots()
        {
            var totalDays = (int)(EndDate - StartDate).Value.TotalDays;
            Slots = totalDays * WorkTimeIds.Length;
        }

        public RosterVm()
        {

        }
        
        public RosterVm(Schedule sch)
        {

            CreateLineItems();
            StartDate = DateTime.Now;
            EndDate = DateTime.Now.AddDays(20);
            SelectedMenu = sch?.SelectedMenu ?? RosterCreateLineItemType.Error;
            Status = sch?.Status ?? ScheduleStatus.Draft;
            //EmployeePayAdjustmentTotals = new List<EmployeePayAdjustmentTotal>();
            ParentScheduleId = sch.ParentScheduleId;
            ParentScheduleName = sch.ParentSchedule?.GetName();
            NextChildScheduleId = sch.FololwingSchedules.FirstOrDefault()?.Id;
            NextChildScheduleName = sch.FololwingSchedules.FirstOrDefault()?.GetName();
            EmployeeActiveContractFlag = sch.HaveConflicts;

            ScheduleId = sch?.Id ?? -1;
            Name = sch?.Name ?? "";
            Header = sch?.GetHeader(sch.SelectedMenu) ?? "Reset";
            StartDate = sch?.Start ?? DateTime.Now.Date;
            EndDate = sch?.End ?? DateTime.Now.AddDays(20).Date;
            EmployeeIds = sch?.EmployeeIds ?? new int[] { 5 };
            Employees = sch?.EmployeeIdsData?.ToList() ?? new List<_Employee> { new _Employee { id = 5, Name = "Reesha", Department = "Exco", Index = 0 } };
            WorkTimeIds = sch?.WorkTimeIds ?? new[] { 1, 2, 3 };
            WorkTimes = sch?.WorkTimeIdsData?.ToList() ?? new List<_WorkTime> { new _WorkTime(0, 1, "Morning", ""), new _WorkTime(1, 2, "Evening", ""), new _WorkTime(2, 3, "Night", "") };

            Slots = sch?.Slots ?? 0;
            Name = sch?.Name;
        }

        private void CreateLineItems()
        {
            RoterCreateMenu = new List<RosterCreateMenu>
            {
                new RosterCreateMenu ("Add Employees and Shifts", RosterCreateLineItemType.AddInitialData),
                new RosterCreateMenu ("Setup Rotating shifts for week", RosterCreateLineItemType.SetRotatingPattern),
                new RosterCreateMenu ("Roster Setup Summary", RosterCreateLineItemType.Summary),
                new RosterCreateMenu ("Verify Employments", RosterCreateLineItemType.SetupEmployee),
                new RosterCreateMenu ("Work arrangements with Shifts", RosterCreateLineItemType.SetupShift),
                //new RosterCreateMenu ("Set Penalties for overall constarnints", RosterCreateLineItemType.Penalties),
                //new RosterCreateMenu ("We're working, Please wait...", RosterCreateLineItemType.Running),
                new RosterCreateMenu ("Master Roster Table", RosterCreateLineItemType.Complete),
            };

            RoterCreateMenu.ForEach(a => SetLink(a));
        }

        private void SetLink(RosterCreateMenu a)
        {
            RoterCreateMenu.ForEach(x => a.Action = "Process"); //  "Step" + (int)a.GetRotaType()
        }
    }
    
    public class RosterCreateMenu
    {
        private readonly string name;
        private readonly RosterCreateLineItemType type;
        private readonly bool isSelected;

        private string link;

        public string Action { get; internal set; }

        public RosterCreateMenu(string name, RosterCreateLineItemType type, bool isSelected = false, string link = "")
        {
            this.name = name;
            this.type = type;
            this.isSelected = isSelected;
            this.link = link;
        }

        public void SetLink(string link) { this.link = link; }
        public string GetName() { return name; }
        public RosterCreateLineItemType GetRotaType() { return type; }

    }
    public enum RosterCreateLineItemType
    {
        AddInitialData = 1,
        SetRotatingPattern = 2,
        SetupEmployee = 3,
        SetupShift = 4,
        //SetupContracts = 4,
        //Penalties = 5,
        Summary = 5,
        //Running = 6,
        Complete = 6,
        Error = 0
    }


    //public class EmployeeRecord
    //{

    //    public Employee Employee { get; set; }
    //    public List<Attendance> Attendances { get; internal set; }
    //    public List<DayVm> DayOffEmployees { get; internal set; }
    //    public List<WorkItem> WorkItems { get; internal set; }

    //    //public DayVm Day { get; set; }

    //    public EmployeeRecord()
    //    {
    //        Attendances = new List<Attendance>();
    //        DayOffEmployees = new List<DayVm>();
    //        WorkItems = new List<WorkItem>();
    //    }

    //}


}
