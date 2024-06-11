using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Service
{
    private static readonly string[] Ones = { "", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
    private static readonly string[] Teens = { "ten", "eleven", "twelve", "Thirteen", "Fourteen", "quarter", "Sixteen", "Seventeen", "Eighteen", "Nineteen" };
    private static readonly string[] Tens = { "", "", "twenty", "half", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
    private static readonly string[] Thousands = { "", "Thousand", "Million", "Billion", "Trillion" };

    public static string ConvertToWords(long number)
    {
        if (number == 0)
            return "Zero";

        string words = "";

        for (int i = 0; number > 0; i++)
        {
            if (number % 1000 != 0)
                words = ConvertHundreds(number % 1000) + Thousands[i] + " " + words;

            number /= 1000;
        }

        return words.Trim();
    }

    private static string ConvertHundreds(long number)
    {
        string words = "";

        if (number >= 100)
        {
            words += Ones[number / 100] + " Hundred ";
            number %= 100;
        }

        if (number >= 10 && number <= 19)
        {
            words += Teens[number % 10] + " ";
        }
        else
        {
            words += Tens[number / 10] + " ";
            words += Ones[number % 10] + " ";
        }

        return words;
    }
}
