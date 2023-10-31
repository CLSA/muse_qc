from fpdf import FPDF
from fpdf.enums import XPos, YPos
import os, sys

"""Summary: A class to for creating pdf reports of various types
            for Muse quality data
"""
class PDF(FPDF):
      font = "Helvetica"
      fontSizeRegular = 14
      fontSizeLarge = int(fontSizeRegular*1.25)
      fontSizeExtraLarge = int(fontSizeRegular*1.5)

      def footer(self):
         # Position at 1.5 cm from bottom
         self.set_y(-15)
         # Arial italic 8
         self.set_font(self.font, 'I', 8)
         # Text color in gray
         self.set_text_color(128)
         # Page number
         self.cell(0, 10, 'Page ' + str(self.page_no()), 0, align = 'C')

      """Summary: Create a report with multiple summary tables
         tables: The tables to be incuded in the report
      """
      def printAllSummaries(self, tables):
         self.add_page()
         for tableData in tables:
            self.printSummaryTable(tableData)

      # table a list of HtmlSummaryTableInfo
      """Summary: Create a report with jpg charts from all participants
         qualityBundles: A bundle of participant info that relates to a certain grouping of quality
      """
      def printSummaryTable(self, tableData):
         # Background color
         self.set_fill_color(0, 0, 0)
         self.set_font_size(12)
         html = generateHtmlForSummaryTable(tableData)
         self.write_html(html)
      
      """Summary: Create a report with jpg charts from all participants
         qualityBundles: A bundle of participant info that relates to a certain grouping of quality
      """
      def printQualityReportImages(self, qualityBundles):
        # Colors of frame, background and text
        self.set_text_color(0,0,0)
        self.set_font(self.font, '', 20)
        
        for qualityBundle in qualityBundles:
            # Create new page with group name as title
            self.add_page()
            self.cell(w = 190, h = 10, txt = qualityBundle.groupName, border = 1, align ='C', new_x= XPos.LEFT, new_y= YPos.NEXT)

            # Add participant jpg files
            self.printQualityReportJpgs(qualityBundle.participants)
        
      """Summary: Displays jpg charts from all participants
         participants: A list of all participants to read jpg path information from
      """
      def printQualityReportJpgs(self, participants):
         self.set_font(self.font, '', self.fontSizeRegular)
         # Colors of frame, background and text
         self.set_text_color(0,0,0)

         jpgLinesDisplayed = 0
         for participant in participants:
            if(jpgLinesDisplayed >= 2):
               self.add_page()
               jpgLinesDisplayed = 0

            # Print weston ID
            self.set_font(self.font, 'BU', self.fontSizeLarge)
            self.cell(h=self.fontSizeExtraLarge, txt = participant.westonID, border = 0, align ='L', new_x= XPos.LEFT, new_y= YPos.NEXT)
            self.set_font(self.font, '', self.fontSizeRegular)
            for i in range(0, len(participant.collections), 2):
               if(jpgLinesDisplayed >= 2):
                  self.add_page()
                  jpgLinesDisplayed = 0
   
               self.displayJpgLine(participant, i)
               jpgLinesDisplayed+=1

      """Summary: Display 1 or 2 muse quality jpg charts in the pdf
         participant: The participant to read jpg path information from
         i: The starting value of the first collection to read from
      """
      def displayJpgLine(self, participant, i):
         width = 90
         height = 6
         imgPadding = 2

         firstCollection = participant.collections[i]
         secondRowToDisplay = len(participant.collections) > i+1
         # Cell 1
         xCell1Start = self.x
         cell1NewX = XPos.LEFT
         cell1NewY = YPos.NEXT
         if(secondRowToDisplay):
            cell1NewX = XPos.RIGHT
            cell1NewY = YPos.TOP
         cell1Text = "Start Date: {}\nQuality: {}\nDuration: {}".format(firstCollection.startDateTime,firstCollection.qualityProblem ,firstCollection.durationProblem)
         self.multi_cell(width, height, cell1Text, 0, align = 'L', new_x= cell1NewX, new_y=cell1NewY)

         # cell 2
         # NOTE: If there is only one jpg to display then cell 2 and image 2 are skipped
         if(secondRowToDisplay):
            xCell2Start = self.x
            secondCollection = participant.collections[i + 1]
            cell2Text = "Start Date: {}\nQuality: {}\nDuration: {}".format(secondCollection.startDateTime,secondCollection.qualityProblem ,secondCollection.durationProblem)
            self.multi_cell(width, height, cell2Text, 0, align = 'L', new_x= XPos.LEFT, new_y=YPos.NEXT)

         # image 1
         self.x = xCell1Start
         yimg1 = self.y
         self.image(firstCollection.path, x = xCell1Start + imgPadding, w = width - imgPadding*2)

         # image 2
         if(secondRowToDisplay):
            self.image(secondCollection.path, x = xCell2Start + imgPadding, y = yimg1, w = width - imgPadding*2)

#region HTML Summary Table code

"""Summary: Generates a string containing html code to display a table 
            of muse summary data 
   tableData: An HtmlSummaryTableInfo object containing data to display
"""
def generateHtmlForSummaryTable(tableData):
   htmlStr = """<h1>""" + tableData.title + """</h1>"""
   htmlStr += """<table border="1">"""
   htmlStr += generateHtmlSummaryTableHeader(tableData)
   htmlStr += """<tbody>"""
   htmlStr += generateHtmlSummaryTableBody(tableData)
   htmlStr += """</tbody>"""
   htmlStr += """</table>"""
   return htmlStr

"""Summary: Generates a string containing html code for the header 
            of a muse summary data table
   tableData: An HtmlSummaryTableInfo object containing data to display
"""
def generateHtmlSummaryTableHeader(tableData):
   headingList = getHeadingList(tableData)
   paramColWidth = 12
   daysColWidth = int((100.0-paramColWidth)/len(headingList))
   html = """
            <thead>
               <tr>
                  <th width=""" + str(paramColWidth) + """%></th>"""
   for heading in headingList:
      html += """                  <th width=""" + str(daysColWidth) + """%>""" + heading + """</th>"""
   html += """
               </tr>
            </thead>
            """
   return  html 

"""Summary: Gets a list of all of the headings from each table entry
   tableData: An HtmlSummaryTableInfo object containing data to display
"""
def getHeadingList(tableData):
   headings = []
   for tableEntry in tableData.entries:
      headings.append(tableEntry.label)
   return headings

"""Summary: Generates a string containing html code for the body 
            of a muse summary data table
   tableData: An HtmlSummaryTableInfo object containing data to display
"""
def generateHtmlSummaryTableBody(tableData):
   htmlRowEnd = "</tr>"
   bgColour = "#f0f0f5"
   bgIssueColour = "#ffebe6"
   bgGoodColour = "#ecf9ec"
   bgSummaryColour = "#b3e0ff"

   # Add n
   html = """<tr bgcolor='""" + bgColour + """'>
      <th align="center">n</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + str(entry.total) + """</td>"""
   html += htmlRowEnd

   # Add 0 days
   html += """<tr>
      <th align="center" bgcolor='""" + bgIssueColour + """'>0 Days</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getD0Str() + """</td>"""
   html += htmlRowEnd

   # Add 1 day
   html += """<tr bgcolor='""" + bgColour + """'>
      <th align="center" bgcolor='""" + bgIssueColour + """'>1 Day</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getD1Str() + """</td>"""
   html += htmlRowEnd

   # Add 2 days
   html += """<tr>
      <th align="center" bgcolor='""" + bgIssueColour + """'>2 Days</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getD2Str() + """</td>"""
   html += htmlRowEnd

   # Add 3 days
   html += """<tr bgcolor='""" + bgColour + """'>
      <th align="center" bgcolor='""" + bgGoodColour + """'>3 Days</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getD3Str() + """</td>"""
   html += htmlRowEnd

   # Add 4+ days
   html += """<tr>
      <th align="center" bgcolor='""" + bgGoodColour + """'>4+ Days</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getD4PlusStr() + """</td>"""
   html += htmlRowEnd

   # Add Duration Problem
   html += """<tr bgcolor='""" + bgColour + """'>
      <th align="center" bgcolor='""" + bgIssueColour + """'>Duration</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getDurationStr() + """</td>"""
   html += htmlRowEnd

   # Add Quality Problem
   html += """<tr>
      <th align="center" bgcolor='""" + bgIssueColour + """'>Quality</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getQualityStr() + """</td>"""
   html += htmlRowEnd

   # Add < 3 Files
   html += """<tr bgcolor='""" + bgColour + """'>
      <th align="center" bgcolor='""" + bgIssueColour + """'>Missing</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getLessThan3Str() + """</td>"""
   html += htmlRowEnd

   # Add Bad total
   html += """<tr>
      <th align="center" bgcolor='""" + bgSummaryColour + """'>0-2 Days</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getBadStr() + """</td>"""
   html += htmlRowEnd

   # Add Good total
   html += """<tr bgcolor='""" + bgColour + """'>
      <th align="center" bgcolor='""" + bgSummaryColour + """'>3+ Days</th>"""
   for entry in tableData.entries:
      html += """<td align="right">""" + entry.getGoodStr() + """</td>"""
   html += htmlRowEnd

   return  html

"""Summary: A class to store data for a single line entry of a table 
            containing muse quality summary data
"""
class HtmlSummaryTableEntry:
   def __init__(self, label,d0,d1,d2,d3,d4Plus,durProb,qualityProb,lessThan3):
      self.label = label
      self.d0 = d0
      self.d1 = d1
      self.d2 = d2
      self.d3 = d3
      self.d4Plus = d4Plus
      self.duration = durProb
      self.quality = qualityProb
      self.lessThan3 = lessThan3
      self.good = self.d3 + self.d4Plus
      self.bad = self.d0 + self.d1 + self.d2
      self.total = self.d0 + self.d1 + self.d2 + self.d3 + self.d4Plus
      self.d0Percent = self.d0 * 100.0 / self.total
      self.d1Percent = self.d1 * 100.0 / self.total
      self.d2Percent = self.d2 * 100.0 / self.total
      self.d3Percent = self.d3 * 100.0 / self.total
      self.d4PlusPercent = self.d4Plus * 100.0 / self.total
      self.goodPercent = self.good * 100.0 / self.total
      self.badPercent = self.bad * 100.0 / self.total
      self.durationPercent = self.duration * 100.0 / self.total
      self.qualityPercent = self.quality * 100.0 / self.total
      self.lessThan3Percent = self.lessThan3 * 100.0 / self.total

   def formatStr(self, val,percent):
      return "{:4d} ({:2.0f}%)".format(val, round(percent,0)) 

   def getD0Str(self):
      return self.formatStr(self.d0, self.d0Percent)
   
   def getD1Str(self):
      return self.formatStr(self.d1, self.d1Percent)
   
   def getD2Str(self):
      return self.formatStr(self.d2, self.d2Percent)
   
   def getD3Str(self):
      return self.formatStr(self.d3, self.d3Percent)
   
   def getD4PlusStr(self):
      return self.formatStr(self.d4Plus, self.d4PlusPercent)
   
   def getGoodStr(self):
      return self.formatStr(self.good, self.goodPercent)
   
   def getBadStr(self):
      return self.formatStr(self.bad, self.badPercent)
   
   def getDurationStr(self):
      return self.formatStr(self.duration, self.durationPercent)
   
   def getQualityStr(self):
      return self.formatStr(self.quality, self.qualityPercent)
   
   def getLessThan3Str(self):
      return self.formatStr(self.lessThan3, self.lessThan3Percent)

"""Summary: A class to store data for a a table 
            containing muse quality summary data
"""
class HtmlSummaryTableInfo:
   def __init__(self, title, entries):
      self.title = title
      self.entries = entries

   def addEntry(self, entry):
      self.entries.append(entry)
   
   def reduceEntries(self, maxTables):
      if(len(self.entries) > maxTables):
         self.entries = self.entries[:maxTables]

"""Summary: Reads muse quality summary table data for multiple tables from a
            single csv file
   path: The path to the csv containing muse quality summary table data
   returns: a HtmlSummaryTableInfo object conatining the data read in 
"""
def generateTables(path):
   f = open(path, "r")
   lines = f.readlines()
   f.close()

   tables = []
   for line in lines[1:]:
      lineSplit = line.split(",")
      if(lineSplit[0] != ""):
         tables.append(HtmlSummaryTableInfo( lineSplit[0], []))

      heading = lineSplit[1]
      days0 = int(lineSplit[2])
      days1 = int(lineSplit[3])
      days2 = int(lineSplit[4])
      days3 = int(lineSplit[5])
      days4Plus = int(lineSplit[6])
      duration = int(lineSplit[7])
      quality = int(lineSplit[8])
      collected = int(lineSplit[9])
      newEntry = HtmlSummaryTableEntry(heading,days0,days1,days2,days3,days4Plus,duration,quality,collected)
      tables[-1].addEntry(newEntry)

      for table in tables:
         table.reduceEntries(8)
   return tables

"""Summary: Creates a summary report based on information stored in a csv
   inPath: The path tot he csv to read from
   outFolder: The folder to store the output file in
"""
def createSummaryReport(inPath, outFolder):
   fileNameWithExtension = os.path.split(inPath)[-1]
   title = fileNameWithExtension.split(".")[0]

   pdf = PDF()
   pdf.set_title(title)
   tables = generateTables(inPath)
   pdf.printAllSummaries(tables)
   pdf.output(outFolder + "/" + title + '.pdf')

#endregion

#region JPG reports code

"""Summary: Stores data about a single muse collection containing a jpg quality file
   path: A path to a muse quality jpg
   startDateTime: The start date of the data collection
   qualityP: True if the collection has a quality problem, false otherwise
   durationP: True if the collection has a duration problem, false otherwise
"""
class MuseQualityCollection:
    def __init__(self, path, startDateTime, qualityP, durationP):
        self.path = path
        self.startDateTime = startDateTime
        self.qualityProblem = "Yes" if qualityP else "No"
        self.durationProblem = "Yes" if durationP else "No"


"""Summary: Stores data about a group of muse collections for a single participant
   westonID: The weston id for the participant
   collections: The collections recorded for this participant that passed through quality checking
"""
class MuseQualityParticipantCollections:
    def __init__(self, wid):
        self.westonID = wid
        self.collections = []

    def appendCollection(self, collection):
        self.collections.append(collection)

"""Summary: stores info about a group of participants with muse collections
   groupName: The group name for this group of data (ex. Problems, good collections, etc)
   participants: The participant info for participants that belong to this group
"""
class MuseQualityBundle:
    def __init__(self, groupName):
        self.groupName = groupName
        self.participants = []

    def appendParticipant(self, participant):
        self.participants.append(participant)

    def getLastParticipantWid(self):
        if(len(self.participants) > 0):
            return self.participants[-1].westonID
        else:
            return ""
        
    def appendCollectionLastParticipant(self, collection):
        self.participants[-1].appendCollection(collection)

"""Summary: reads a csv containing muse quality bundles
   path: The path tot he csv to read from
   returns: A list of MuseQualityBundle
"""
def readMuseBundlesCsv(path):
    f = open(path, "r")
    lines = f.readlines()
    f.close()

    bundles = []
    for line in lines[1:]:
        lineSplit = line.split(",")
        if(lineSplit[0] != ""):
            groupName = lineSplit[0].strip()
            bundles.append(MuseQualityBundle( groupName))
            continue

        wid = lineSplit[1]
        jpgPath = lineSplit[2]
        startDate = lineSplit[3]
        qualityP = lineSplit[4].strip().lower() == "true"
        durationP = lineSplit[5].strip().lower() == "true"
        collection = MuseQualityCollection(jpgPath, startDate, qualityP, durationP)
        
        # if wid different than previous, add new participant
        if(bundles[-1].getLastParticipantWid() != wid):
            bundles[-1].appendParticipant(MuseQualityParticipantCollections(wid))
        
        # add collection to newest participant
        bundles[-1].appendCollectionLastParticipant(collection)
    return bundles
        
"""Summary: Creates a JPG report based on information stored in a csv
   inPath: The path tot he csv to read from
   outFolder: The folder to store the output file in
"""
def createJPGReport(inPath, outFolder):
    pdf = PDF()
    museBundles = readMuseBundlesCsv(inPath)

    fileNameWithExtension = os.path.split(inPath)[-1]
    title = fileNameWithExtension.split(".")[0]
    pdf.set_title(title)
    pdf.printQualityReportImages(museBundles)
    pdf.output(outFolder + "/" + title + '.pdf')

#endregion
   

"""_summary_: Use command line args to call report creation functions
              Expects 3 arguments
              1. Report type: The type of report to create. (Capitalization does not matter)
                  i. summary - to create a summary report
                  ii. jpg - to create a report displaying quality jpg images
              2. Csv Path: The full path to the csv to read information from
              3. Output Dir path: The full path to the directory to store output file in

              Example: MuseQualityReportPdfCreation.py summary c:/MyDir/QualityReport_September_2023.csv c:/MyOutDir
"""
if __name__ == "__main__":
   # read arguments in
   reportType = sys.argv[1]
   inPath = sys.argv[2]
   outFolder = sys.argv[3]

   # decide which report type to try and create
   if(reportType.lower() == "summary"):
      createSummaryReport(inPath, outFolder)
   elif(reportType.lower() == "jpg"):
      createJPGReport(inPath, outFolder)