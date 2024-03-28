using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.IO;


public class DataExportTest
{
    EnvController env;
    [SetUp]
    public void Setup()
    {
        env = new GameObject().AddComponent<EnvController>();
        
    }

    [Test]
    public void ExportDataToCSVTest()
    {
        // Preparing test data
        string[] rowData = new string[]
        {
            "winner",
            "10",
            "5",
            "Alive",
            "Dead",
            Random.value.ToString(),
            "6"
        };

        env.dataRows.Add(rowData);
        // Exporting data to csv file
        env.ExportToCSV("TestExport");

        // Read data from file
        string directoryPath = Application.dataPath + "/DataExport";
        string filePath = directoryPath + "/TestExport.csv";
        string[] fileContents = File.ReadAllLines(filePath);

        // Assert the last line of the file is the same as the rowData
        string lastRow = fileContents[fileContents.Length - 1];
        Assert.AreEqual(string.Join(",", rowData), lastRow);


    }

    [Test]
    public void SaveRoundDataTest()
    {
        Assert.IsEmpty(env.dataRows);
        float randomValue = Random.value;
        string[] data = new string[]
        {
            "Blue", "2", "4", "Dead", "Alive", randomValue.ToString(), "4.5"
        };
        env.SaveRoundData("Blue", 2, 4, false, true, randomValue, 4.5f);
        Assert.IsNotEmpty(env.dataRows);
        Assert.AreEqual(env.dataRows[0], data);
    }

}
