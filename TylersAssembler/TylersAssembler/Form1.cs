using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;



namespace TylersAssembler
{
    public partial class Form1 : Form
    {
        private string FileName;
        private string SavedFileName;
        private bool alreadyHaveAFileSaved;

        public Form1()
        {
            InitializeComponent();
        }

        private void FileLoadButton_Click(object sender, EventArgs e)
        {
            //this.openToolStripMenuItem_Click(sender, e);


            //clear the AssemblyList 
            AssemblyListBox.Items.Clear();


            //Create Instance of the file dialog box
            OpenFileDialog openFileDialog = new OpenFileDialog();

            //set filter options
            openFileDialog.Filter = "Assembler Source (*.asm)|.asm | All Files (*.*)|*.*";

            openFileDialog.Multiselect = false;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.FileName = openFileDialog.FileName;
                foreach (string line in File.ReadLines(openFileDialog.FileName)) 
                {
                    AssemblyListBox.Items.Add(line);
                }
            }


        }

        private void AssembleFileButton_Click(object sender, EventArgs e)
        {
            if (this.FileName != "" && this.FileName != null)
            {
                //Clear the Assembled list box
                AssembledListBox.Items.Clear();

                //AssembledListBox.Items.Add("STARTING TO ASSEMBLE");
                //variables used in parsing process
                //
                //

                string lineLabel = "";           //label from the assembly code
                string lineInstruction = "";     //instruction from assembly code
                string lineReference = "";       //Reference from from assembly code
                string assembledLine = "";       //Final Assembled Line
                string tempString = "";          //temporary string used for various purposes
                int tempInt = 0;                 //temp int for various purposes
                bool hasLabel = false;
                bool hasReference = false;
                bool isIndirect = false;
                bool doneWithLine = false;
                Dictionary<string, string> LabelList = new Dictionary<string, string>();

                int assembledMemoryLine = 0;    //memory location in decimal





                //get the labels that are not LOP and store them in our Dictionary
                //PASS 1 OF ASSEMBLER
                //
                //
                //

                foreach (string line in File.ReadLines(this.FileName))
                {
                    if (line.Length > 4)  //handle blank lines in the asm file
                    {
                        lineLabel = line.Substring(0, 4);
                        lineInstruction = line.Substring(5, 3);
                        //get the right LineReference length
                        if (line.Length < 10)
                        {
                            lineReference = "     ";
                        }
                        else if (line.Length == 10)
                        {
                            lineReference = (line.Substring(9, 1) + "    ");
                        }
                        else if (line.Length == 11)
                        {
                            lineReference = (line.Substring(9, 2) + "   ");
                        }
                        else if (line.Length == 12)
                        {
                            lineReference = (line.Substring(9, 3) + "  ");
                        }
                        else if (line.Length == 13)
                        {
                            lineReference = line.Substring(9, 4) + " ";
                        }
                        else if (line.Length > 13)
                        {
                            lineReference = line.Substring(9, 5);
                        }

                        //CODE FOR CHANGING MEMORY ADDRESS VALUES
                        //
                        //
                        if (lineInstruction == "ORG")
                        {
                            if (lineReference.Substring(0, 3) != "000")
                            {
                                assembledMemoryLine = Convert.ToInt32(lineReference.Substring(0, 3), 16);
                            }
                            else
                            {
                                assembledMemoryLine = 0;
                            }
                            doneWithLine = true;
                        }


                        //Code for adding labels to our label dictionary
                        //The first item or "key" is the name of the label, the second item or 
                        //"value" is the memory address where that label is found.
                        //
                        //
                        //A line with a comma has a label
                        if (lineLabel[3] == ',')
                        {
                            LabelList.Add(lineLabel.Substring(0, 3), assembledMemoryLine.ToString("X3"));
                        }


                        //increment the assembled memory line if the command was not ORG
                        if (!doneWithLine)
                        {
                            assembledMemoryLine++;
                        }
                        doneWithLine = false;
                    }

                }
                //AssembledListBox.Items.Add("FIRST PASS DONE");


                //SECOND PASS OF THE ASSEMBLER
                //THIS WHERE ALL THE ACTUAL PARSING IS DONE
                //
                //
                //
                //
                foreach (string line in File.ReadLines(this.FileName))
                {
                    //if (line.Length > 4)
                    //{  //handle blank lines in the asm file
                        doneWithLine = false;
                        hasReference = false;
                        isIndirect = false;
                        lineLabel = line.Substring(0, 4);
                        lineInstruction = line.Substring(5, 3);

                        if (line.Length < 10)
                        {
                            lineReference = "     ";
                        }
                        else if (line.Length == 10)
                        {
                            lineReference = (line.Substring(9, 1) + "    ");
                        }
                        else if (line.Length == 11)
                        {
                            lineReference = (line.Substring(9, 2) + "   ");
                        }
                        else if (line.Length == 12)
                        {
                            lineReference = (line.Substring(9, 3) + "  ");
                        }
                        else if (line.Length == 13)
                        {
                            lineReference = line.Substring(9, 4) + " ";
                        }
                        else if (line.Length > 13)
                        {
                            lineReference = line.Substring(9, 5);
                        }


                        //check to see if a label is present
                        if (lineLabel[3] == ',')
                        {
                            hasLabel = true;
                        }
                        else
                        {
                            hasLabel = false;
                        }

                        //Check to see if there is a reference
                        if (lineReference[0] != ' ')
                        {
                            hasReference = true;
                        }

                        //check to see if the reference is indirect
                        if (lineReference[4] == 'I')
                        {
                            isIndirect = true;
                        }

                        //set the current memory line if there is an ORG statement
                        if (lineInstruction.Substring(0, 3) == "ORG")
                        {
                            //convert the memory location from hex to dec  
                            assembledMemoryLine = Convert.ToInt32(lineReference.Substring(0, 3), 16);

                            //this line is done if there is an org statemert (no commands)
                            doneWithLine = true;
                        }
                        if (lineInstruction.Substring(0, 3) == "END")
                        {
                            doneWithLine = true;
                        }


                        //Write out the binary code for that instruction
                        //FORMAT:   "XYZ:      VVVV"
                        // where "XYZ" is memory location and "VVVV" is the binary instruction
                        //There are 6 spaces between the colon and the binary instruction
                        if (!doneWithLine)
                        {
                            //add the memory location
                            assembledLine = assembledMemoryLine.ToString("X3");
                            assembledLine += ":      ";
                            assembledMemoryLine++;

                            //check to see if there is a label without an assigned address
                            if (hasLabel && lineInstruction.Substring(0, 3) != "HEX" && lineInstruction.Substring(0, 3) != "DEC")
                            {
                                //set the value of the label to the assembledMemoryLine in hex 
                                LabelList[lineLabel.Substring(0, 3)] = (assembledMemoryLine - 1).ToString("X3");
                            }

                            //Check for memory reference instructions
                            switch (lineInstruction.Substring(0, 3))
                            {
                                case "AND":
                                    //add the first digit
                                    assembledLine += (isIndirect) ? "8" : "0";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;
                                case "ADD":
                                    assembledLine += (isIndirect) ? "9" : "1";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;

                                case "LDA":
                                    assembledLine += (isIndirect) ? "A" : "2";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;

                                case "STA":
                                    assembledLine += (isIndirect) ? "B" : "3";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;

                                case "BUN":
                                    assembledLine += (isIndirect) ? "C" : "4";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;

                                case "BSA":
                                    assembledLine += (isIndirect) ? "D" : "5";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;

                                case "ISZ":
                                    assembledLine += (isIndirect) ? "E" : "6";
                                    //add the memory reference to the end of the instruction
                                    assembledLine += CheckLabelListForMatch(LabelList, lineReference);
                                    break;

                                default:
                                    break;

                            }

                            //Check for register reference or I/O instructions
                            switch (lineInstruction.Substring(0, 3))
                            {
                                case "CLA":
                                    assembledLine += "7800";
                                    break;
                                case "CLE":
                                    assembledLine += "7400";
                                    break;
                                case "CMA":
                                    assembledLine += "7200";
                                    break;
                                case "CME":
                                    assembledLine += "7100";
                                    break;
                                case "CIR":
                                    assembledLine += "7080";
                                    break;
                                case "CIL":
                                    assembledLine += "7040";
                                    break;
                                case "INC":
                                    assembledLine += "7020";
                                    break;
                                case "SPA":
                                    assembledLine += "7010";
                                    break;
                                case "SNA":
                                    assembledLine += "7008";
                                    break;
                                case "SZA":
                                    assembledLine += "7004";
                                    break;
                                case "SZE":
                                    assembledLine += "7002";
                                    break;
                                case "HLT":
                                    assembledLine += "7001";
                                    break;
                                case "INP":
                                    assembledLine += "F800";
                                    break;
                                case "OUT":
                                    assembledLine += "F400";
                                    break;
                                case "SKI":
                                    assembledLine += "F200";
                                    break;
                                case "SKO":
                                    assembledLine += "F100";
                                    break;
                                case "ION":
                                    assembledLine += "F080";
                                    break;
                                case "IOF":
                                    assembledLine += "F040";
                                    break;
                                default:
                                    break;
                            }

                            //Check for DEC and HEX contents
                            switch (lineInstruction.Substring(0, 3))
                            {
                                case "HEX":
                                    //add the label and its value
                                    tempString = lineReference.Substring(0, 3);
                                    //convert to decimal, then back to hex, to get the right format
                                    tempInt = int.Parse(tempString, System.Globalization.NumberStyles.HexNumber);
                                    tempString = tempInt.ToString("X4");
                                    assembledLine += tempString;
                                    break;
                                case "DEC":
                                    //convert decimal number to hex
                                    tempInt = Convert.ToInt32(lineReference.Substring(0, 3));
                                    tempString = tempInt.ToString("X4");
                                    if (tempInt >= 0)
                                    {
                                        tempString = tempInt.ToString("X4");
                                    }
                                    else
                                    {
                                        //only take the last 3 hex numbers in the string
                                        tempString = tempString.Substring(tempString.Length - 4, 4);
                                    }
                                    assembledLine += tempString;
                                    break;
                                default:
                                    break;
                            }


                            //add assembled line to the listBox
                            AssembledListBox.Items.Add(assembledLine);

                            //clear assembled line and other variables for next iteration
                            assembledLine = "";
                            lineLabel = "";
                            lineInstruction = "";
                            lineReference = "";


                        }
                    

                }

            }
        }


        //FUNCTION USED TO SEARCH DICTIONARY
        //
        //
        private string CheckLabelListForMatch(Dictionary<string, string> labelList, string reference) 
        {
            foreach (KeyValuePair<string, string> label in labelList) 
            {
                if (label.Key == reference.Substring(0, 3))
                {
                    //return the address value of that label
                    return label.Value;
                }
            }
            //if there is not a label associated with the reference
            //i.e. they put ADD 265, instead of ADD PTR
            return reference.Substring(0, 3);
        }


        //OPEN FILE OPTION IN MENU
        //
        //
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //clear the AssemblyList 
            AssemblyListBox.Items.Clear();


            //Create Instance of the file dialog box
            OpenFileDialog openFileDialog = new OpenFileDialog();

            //set filter options
            //openFileDialog.Filter = "Assembly Files (*.asm)|.asm";

            openFileDialog.Multiselect = false;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                this.FileName = openFileDialog.FileName;

                //MAKE SURE THIS FILE IS AN ASSEMBLY FILE
                if (this.FileName.Substring(this.FileName.Length - 4, 4) == ".asm")
                {
                    foreach (string line in File.ReadLines(openFileDialog.FileName))
                    {
                        AssemblyListBox.Items.Add(line);
                    }
                }
                else 
                {
                    MessageBox.Show("File Must Be An Assembler Source (.asm) File", "File Type Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.FileName = "";
                }
            }


        }


        //SELECT FILE BUTTON IN MAIN PROGRAM
        //
        //
        private void SelectFileButton_Click(object sender, EventArgs e)
        {

            this.openToolStripMenuItem_Click(sender, e);           

        }


        //SAVE AS MENU OPTION
        //
        //
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "CDM File (*.cdm)|*.cdm| All Files (*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            DialogResult result = saveFileDialog.ShowDialog();
            this.SavedFileName = saveFileDialog.FileName;

            if (result == DialogResult.OK)
            {
                //Write the Assembled Code to the new file.
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@SavedFileName))
                {
                    foreach (string line in AssembledListBox.Items)
                    {
                        file.WriteLine(line.Substring(0, 4) + line.Substring(10, 4));
                    }
                }
                MessageBox.Show("File Saved", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.alreadyHaveAFileSaved = true;
            }

        }


        //MENU SAVE SELECTION
        //
        //
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.alreadyHaveAFileSaved)
            {
                //Write the Assembled Code to the new file.
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@SavedFileName))
                {
                    foreach (string line in AssembledListBox.Items)
                    {
                        file.WriteLine(line.Substring(0, 4) + line.Substring(10, 4));
                    }
                }
                //LET THE USER KNOW THE FILE HAS BEEN SAVED
                MessageBox.Show("File Saved", "Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else 
            {
                //IF THERE HASN'T BEEN A FILE NAME SPECIFIED YET USE SAVE AS INSTRUCTION
                this.saveAsToolStripMenuItem_Click(sender, e);
            }
        }


        //EXIT MENU SELECTION
        //
        //
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void SaveFileButton_Click(object sender, EventArgs e)
        {
            this.saveToolStripMenuItem_Click(sender, e);
        }


        //EXIT BUTTON
        //
        //
        private void ExitButton_Click(object sender, EventArgs e)
        {
            //USE DIALOG TO PREVENT ACCIDENTAL CLICKING OF THE BUTTON
            DialogResult dialog = MessageBox.Show("Are you sure?", "Close", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialog == DialogResult.OK) 
            {
                Application.Exit();
            }
            else if (dialog == DialogResult.Cancel)
            {
                
            }
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult dialog = MessageBox.Show("This program is used to convert Assebler Source files to a \".cdm\", " +
                "or Cedar Logic Memory file. To use the program open a \".asm\" file, and press the Assemble button, the " +  
                "binary code will be visible to the right. Lastly, you can save the converted cdm file by pressing the save " +
                "button, or by using the file menu option.", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                             
        } 


    }
}
