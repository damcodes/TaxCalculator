using System;
using System.Collections.Generic;
using System.IO;

namespace TaxCalculator
{
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = @"./employees.csv";
            List<Employee> employees = GetCreateEmployees(filePath);
            bool cont = true;
            while (cont)
            {
                Console.WriteLine("---------------------------------------------------------------------");
                SortMenu();
                string answer = Console.ReadLine();
                Console.WriteLine("\n");
                Console.Clear();
                switch (answer)
                {

                    case "1":
                        employees.Sort(CompareEmployeesByName);
                        DisplayEmployeDataTable(employees);
                        break;
                    case "2":
                        employees.Sort(CompareEmployeesById);
                        DisplayEmployeDataTable(employees);
                        break;
                    case "3":
                        employees.Sort(CompareEmployeesByState);
                        DisplayEmployeDataTable(employees);
                        break;
                    case "4":
                        employees.Sort(CompareEmployeesBySalary);
                        DisplayEmployeDataTable(employees);
                        break;
                    case "5":
                        employees.Sort(CompareEmployeesByTaxesDue);
                        DisplayEmployeDataTable(employees);
                        break;
                    default:
                        cont = false;
                        Console.WriteLine("Goodbye.");
                        break;
                }
            }
        }

        static List<Employee> GetCreateEmployees(string filePath)
        {
            List<Employee> employees = new();
            StreamReader reader = new(File.OpenRead(filePath));

            while (!reader.EndOfStream)
            {
                string csv = reader.ReadLine();
                try
                {
                    Employee newEmp = ValidateCreateEmployee(csv);
                    employees.Add(newEmp);
                }
                catch
                {
                    continue;
                }
            }

            return employees;
        }

        static Employee ValidateCreateEmployee(string csv)
        {
            Employee newEmp;
            string[] data = csv.Split(',');
            string name = data[1].Trim();
            string stateCode = data[2].Trim();
            int id;
            decimal hoursWorked;
            decimal rate;

            if (int.TryParse(data[0], out id) & decimal.TryParse(data[3], out hoursWorked) & decimal.TryParse(data[4], out rate))
            {
                newEmp = new(id: id, name: name, stateCode: stateCode, hoursWorked: hoursWorked, rate: rate);
            }
            else
            {
                throw new ArgumentException($"Hours worked and rate must be valid numbers. Either '{hoursWorked}' or '{rate}' is invalid.");
            }
            return newEmp;
        }

        static void SortMenu()
        {
            Console.WriteLine("Enter a number to sort the employee data. Enter anything else to exit.\n");
            Console.WriteLine("\t\t1. By Name");
            Console.WriteLine("\t\t2. By ID");
            Console.WriteLine("\t\t3. By State");
            Console.WriteLine("\t\t4. By Salary");
            Console.WriteLine("\t\t5. By Taxes Due");
            Console.Write("==> ");
        }

        static int CompareEmployeesByName(Employee a, Employee b)
        {
            if (a.Name == null)
            {
                if (b.Name == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (b.Name == null)
                {
                    return 1;
                }
                else
                { 
                    return a.Name.CompareTo(b.Name);
                }
            }
        }

        static int CompareEmployeesById(Employee a, Employee b)
        {
            return b.Id.CompareTo(a.Id);
        }

        static int CompareEmployeesByState(Employee a, Employee b)
        {
            if (a.StateCode == null)
            {
                if (b.StateCode == null)
                {
                    return 0;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
                if (b.StateCode == null)
                {
                    return 1;
                }
                else
                {
                    return a.StateCode.CompareTo(b.StateCode);
                }
            }
        }

        static int CompareEmployeesBySalary(Employee a, Employee b)
        {
            if (b.TotalWages() > a.TotalWages())
            {
                return 1;
            }
            else if (a.TotalWages() > b.TotalWages())
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        static int CompareEmployeesByTaxesDue(Employee a, Employee b)
        {
            if (b.CalculateTax() > a.CalculateTax())
            {
                return 1;
            }
            else if (a.CalculateTax() > b.CalculateTax())
            {
                return -1;
            }
            else
            { return 0; }
        }

        static void TableHeader()
        {
            Console.WriteLine($"{"ID",-5}{"Employee",-12}{"State",-10}{"Hours Worked",-15}{"Rate",-8}{"Total Wages",-15}{"Taxes Due",-15}");
            Console.WriteLine("--------------------------------------------------------------------------");
        }

        static void DisplayEmployeDataTable(List<Employee> employees)
        {
            TableHeader();
            foreach (Employee employee in employees)
            {
                Console.WriteLine($"{employee.Id,-5}{employee.Name,-12}{employee.StateCode,-12}{employee.HoursWorked,-13}" +
                    $"{$"${employee.Rate}",-7:0.00}{$"${employee.TotalWages()}",11}{$"${employee.CalculateTax()}",13}");
            }
        }
    }

    public class TaxCalculator
    {
        private readonly State state;
        private readonly List<StateTaxInfo> stateTaxInfoList;
        private bool verbose;

        public TaxCalculator(string code = "", string name = "")
        {
            string filePath = @"./taxtable.csv";
            List<StateTaxInfo> allStateTaxInfoList = GetStateData(filePath);
            stateTaxInfoList = allStateTaxInfoList.FindAll(data => (data.Code == code) || (data.Name == name));
            string stateCode = stateTaxInfoList[0].Code;
            string stateName = stateTaxInfoList[0].Name;
            state = new(stateCode, stateName);
            verbose = false;
        }

        public bool Verbose
        {
            get { return verbose; }

            set { verbose = value; }
        }

        static List<StateTaxInfo> GetStateData(string filePath)
        {
            List<StateTaxInfo> states = new();
            StreamReader reader = new(File.OpenRead(filePath));
            while (!reader.EndOfStream)
            {
                string csv = reader.ReadLine().Trim();
                try
                {
                    StateTaxInfo newState = ValidateCreateStateTaxInfo(csv);
                    states.Add(newState);
                }
                catch (Exception)
                {
                    //Console.WriteLine(e.Message + '\n');
                    continue;
                }
            }
            return states;
        }

        static StateTaxInfo ValidateCreateStateTaxInfo(string csv)
        {
            StateTaxInfo newStateTaxInfo;
            string[] data = csv.Split(',');

            ArraySegment<string> attrs = new(data, 0, data.Length - 1);
            if (attrs.Count == 5)
            {
                string code = attrs[0];
                string name = attrs[1];
                decimal floor;
                decimal ceiling;
                decimal taxRate;

                if (decimal.TryParse(attrs[2], out floor)
                    & decimal.TryParse(attrs[3], out ceiling)
                    & decimal.TryParse(attrs[4], out taxRate))
                {
                    newStateTaxInfo = new(code, name, floor, ceiling, taxRate);
                }
                else
                {
                    throw new Exception($"Either '{attrs[2]}', '{attrs[3]}', or '{attrs[4]}' is an invalid decimal values. This StateTaxInfo for \"{name}\" not created.");
                }
            }
            else
            {
                throw new ArgumentException("Incorrect number of arguments to create StateTaxInfo object. StateTaxInfo object not created.");
            }
            return newStateTaxInfo;
        }

        public decimal ComputeTaxFor(decimal amountEarned)
        {
            decimal tax = 0;
            List<StateTaxInfo> cappedTaxBrackets = stateTaxInfoList.FindAll(data => amountEarned > data.Ceil || (data.Floor < amountEarned && amountEarned < data.Ceil));

            int taxBracketCount = cappedTaxBrackets.Count;
            switch (taxBracketCount)
            {
                case > 1:
                    tax = CalcTaxForMultiBracket(verbose: verbose, stateTaxObjects: cappedTaxBrackets, state: this.state, originalIncome: amountEarned);
                    break;
                default:
                    tax = CalcTaxForOneBracket(verbose: verbose, stateTaxObjects: cappedTaxBrackets, state: this.state, originalIncome: amountEarned);
                    break;
            }
            return tax;
        }

        static decimal CalcTaxForMultiBracket(bool verbose, List<StateTaxInfo> stateTaxObjects, State state, decimal originalIncome)
        {
            decimal ogIncome = originalIncome;
            decimal tax = 0;
            switch (verbose)
            {
                case true:
                    Console.WriteLine("==================================================\n");
                    Console.WriteLine($"Computing state tax for ${originalIncome:0.00} earned in {state.Name}....\n");
                    Console.WriteLine($"{state.Name} has {stateTaxObjects.Count} tax brackets for your income. To calculate your taxes I must cummulatively " +
                        $"sum the products of each tax rate with the portion of your" +
                        $" income within it's range.\n");

                    for (int i = stateTaxObjects.Count - 1; i >= 0; i--)
                    {
                        StateTaxInfo currTaxBracket = stateTaxObjects[i];
                        tax += (originalIncome - currTaxBracket.Floor) * currTaxBracket.TaxRate;
                        Console.WriteLine($"                  Bracket {i+1}                  ");
                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine($"{"Tax Rate(%)",-10}{currTaxBracket.TaxRate*100,19:0.00}");
                        Console.WriteLine($"{"Floor(USD)",-10}{currTaxBracket.Floor,20:0.00}");
                        Console.WriteLine($"{"Ceil(USD)",-10}{currTaxBracket.Ceil,20:0.00}");
                        Console.WriteLine("\n\n");
                        Console.WriteLine($"{"Taxable Income", -10}{originalIncome - currTaxBracket.Floor,16:0.00}");
                        Console.WriteLine($"{"Tax", -10}{(originalIncome - currTaxBracket.Floor)*(currTaxBracket.TaxRate), 20:0.00}");
                        Console.WriteLine("--------------------------------------------------\n\n\n");

                        originalIncome = currTaxBracket.Floor;
                    }
                    Console.WriteLine($"\nYou'd pay ${tax:0.00} in {state.Name} state taxes on ${ogIncome:0.00}\n");
                    Console.WriteLine("==================================================\n\n\n");
                    break;
                default:
                    for (int i = stateTaxObjects.Count - 1; i > 0; i--)
                    {
                        StateTaxInfo currTaxBracket = stateTaxObjects[i];
                        tax += (originalIncome - currTaxBracket.Floor) * currTaxBracket.TaxRate;
                        originalIncome = currTaxBracket.Floor;
                    }
                    break;
            }
            return tax;
        }

        static decimal CalcTaxForOneBracket(bool verbose, List<StateTaxInfo> stateTaxObjects, State state, decimal originalIncome)
        {
            StateTaxInfo currTaxBracket = stateTaxObjects[0];
            decimal tax = originalIncome * stateTaxObjects[0].TaxRate;
            if (verbose)
            {

                Console.WriteLine("==================================================\n");
                Console.WriteLine($"Computing state tax for ${originalIncome:0.00} earned in {state.Name}....\n");
                Console.WriteLine($"This state has only 1 tax bracket for your income. You claim ${originalIncome:0.00} in {state.Name}. \n" +
                    $"With a tax rate of {stateTaxObjects[0].TaxRate * 100:0.00}% for income between ${stateTaxObjects[0].Floor:0.00} and ${stateTaxObjects[0].Ceil:0.00}, " +
                    $"your taxes are calculated by multiplying your income by the tax rate.\n\ntaxes = " +
                    $"(income)*(tax rate) = ({originalIncome})*({stateTaxObjects[0].TaxRate}) " +
                    $"= ${originalIncome * stateTaxObjects[0].TaxRate:0.00}\n");
                Console.WriteLine($"                   Bracket 1                     ");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"{"Tax Rate(%)",-10}{currTaxBracket.TaxRate * 100,19:0.00}");
                Console.WriteLine($"{"Floor(USD)",-10}{currTaxBracket.Floor,20:0.00}");
                Console.WriteLine($"{"Ceil(USD)",-10}{currTaxBracket.Ceil,20:0.00}");
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"\nYou'd pay ${tax:0.00} in {state.Name} state taxes on ${originalIncome:0.00}\n");
                Console.WriteLine("==================================================\n\n\n");
            }
            return tax;
        }
    }

    public class StateTaxInfo
    {
        private readonly string code;
        private readonly string name;
        private readonly decimal floor;
        private readonly decimal ceiling;
        private readonly decimal taxRate;

        public StateTaxInfo() 
        {
            code = "";
            name = "";
            floor = 0.00M;
            ceiling = 0.00M;
            taxRate = 0.00M;
        }

        public StateTaxInfo(string code, string name, decimal floor, decimal ceiling, decimal taxRate)
        {
            this.code = code;
            this.name = name;
            this.floor = floor;
            this.ceiling = ceiling;
            this.taxRate = taxRate;
        }

        public override string ToString()
        {
            return $"StateTaxInfo object--Code: {code}, Name: {name}, Floor: ${floor}, Ceil: ${ceiling}, Rate: %{taxRate}";
        }

        public string Code
        {
            get { return code; }
        }

        public string Name
        {
            get { return name; }
        }

        public decimal Floor
        {
            get { return floor; }
        }

        public decimal Ceil
        {
            get { return ceiling; }
        }

        public decimal TaxRate
        {
            get { return taxRate; }
        }
    }

    public class State
    {
        private readonly string code;
        private readonly string name;

        public State()
        {
            code = "";
            name = "";
        }

        public State(string code, string name)
        {
            this.code = code;
            this.name = name;
        }

        public string Code
        {
            get { return code; }
        }

        public string Name
        {
            get { return name; }
        }
    }

    public class Employee
    {
        private readonly int id;
        private readonly string name;
        private readonly string stateCode;
        private readonly decimal hoursWorked;
        private readonly decimal rate;

        public Employee(int id = 0, string name = "Employee", string stateCode = "state", decimal hoursWorked = 0, decimal rate = 0)
        {
            this.id = id;
            this.name = name;
            this.stateCode = stateCode;
            this.hoursWorked = hoursWorked;
            this.rate = rate;
        }

        public int Id
        {
            get { return id;  }
        }

        public string Name
        {
            get { return name; }
        }

        public string StateCode
        {
            get { return stateCode; }
        }

        public decimal HoursWorked
        {
            get { return hoursWorked; }
        }

        public decimal Rate
        {
            get { return rate; }
        }

        public decimal TotalWages()
        {
            decimal grossIncome = hoursWorked * rate;
            return decimal.Round(grossIncome, 2);
        }

        public decimal CalculateTax()
        {
            decimal tax = 0;
            TaxCalculator tc = new(code: stateCode);
            //tc.Verbose = true;
            tax = tc.ComputeTaxFor(TotalWages());

            return Decimal.Round(tax, 2);
        }
    }
}
