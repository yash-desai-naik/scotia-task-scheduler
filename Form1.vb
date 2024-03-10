Imports System.IO

Public Class Form1

    Private Sub MainForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        CheckTaskStatus()
    End Sub

    Private Sub CheckTaskStatus()
        Dim taskExists As Boolean = TaskIsScheduled("scotia-email-drafts")

        If taskExists Then
            btnStart.Enabled = False
            btnStop.Enabled = True

            ' Get the next run time of the task and update the status bar
            Dim nextRunTime As String = GetNextRunTime("scotia-email-drafts")
            ToolStripStatusLabel1.Text = "Next run time: " & nextRunTime
            ToolStripStatusLabel1.ForeColor = Color.Black
            If nextRunTime IsNot Nothing Then

            End If
        Else
            btnStart.Enabled = True
            btnStop.Enabled = False
            ToolStripStatusLabel1.Text = "Task not scheduled"
            ToolStripStatusLabel1.ForeColor = Color.Black
        End If
    End Sub

    Private Sub btnStart_Click(sender As Object, e As EventArgs) Handles btnStart.Click
        Dim openFileDialog As New OpenFileDialog()
        openFileDialog.Filter = "Executable Files (*.exe)|*.exe"
        openFileDialog.Title = "Select Executable File"

        If openFileDialog.ShowDialog() = DialogResult.OK Then
            Dim executablePath As String = openFileDialog.FileName
            CreateTask(executablePath)
        End If
    End Sub
    Private Sub CreateTask(executablePath As String)
        Try
            ' Define the XML content for the task definition
            Dim xmlContent As String = $"<?xml version='1.0' encoding='UTF-16'?>
<Task version='1.2' xmlns='http://schemas.microsoft.com/windows/2004/02/mit/task'>
  <RegistrationInfo>
    <Date>2024-03-10T20:38:52</Date>
    <Author>YASH-PC\yash</Author>
    <URI>\scotia-email-drafts</URI>
  </RegistrationInfo>
  <Triggers>
    <CalendarTrigger>
      <StartBoundary>2024-03-10T00:00:00</StartBoundary>
      <Enabled>true</Enabled>
      <ScheduleByMonth>
        <DaysOfMonth>
          <Day>1</Day>
        </DaysOfMonth>
        <Months>
          <January />
          <February />
          <March />
          <April />
          <May />
          <June />
          <July />
          <August />
          <September />
          <October />
          <November />
          <December />
        </Months>
      </ScheduleByMonth>
    </CalendarTrigger>
  </Triggers>
  <Principals>
    <Principal id='Author'>
      <UserId>S-1-5-21-2800104608-1952058749-1654487197-1002</UserId>
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>LeastPrivilege</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>true</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>true</StopIfGoingOnBatteries>
    <AllowHardTerminate>true</AllowHardTerminate>
    <StartWhenAvailable>true</StartWhenAvailable>
    <RunOnlyIfNetworkAvailable>false</RunOnlyIfNetworkAvailable>
    <IdleSettings>
      <StopOnIdleEnd>true</StopOnIdleEnd>
      <RestartOnIdle>false</RestartOnIdle>
    </IdleSettings>
    <AllowStartOnDemand>true</AllowStartOnDemand>
    <Enabled>true</Enabled>
    <Hidden>false</Hidden>
    <RunOnlyIfIdle>false</RunOnlyIfIdle>
    <WakeToRun>false</WakeToRun>
    <ExecutionTimeLimit>PT1H</ExecutionTimeLimit>
    <Priority>7</Priority>
    <RestartOnFailure>
      <Interval>PT1M</Interval>
      <Count>3</Count>
    </RestartOnFailure>
  </Settings>
  <Actions Context='Author'>
    <Exec>
      <Command>{executablePath}</Command>
    </Exec>
  </Actions>
</Task>
        "

            ' Save the XML content to a temporary file
            Dim tempXmlFile As String = Path.Combine(Path.GetTempPath(), "TaskDefinition.xml")
            File.WriteAllText(tempXmlFile, xmlContent)

            ' Create the task using schtasks /create /xml
            Dim processInfo As New ProcessStartInfo("schtasks", $"/Create /XML ""{tempXmlFile}"" /TN ""scotia-email-drafts""")
            processInfo.CreateNoWindow = True
            processInfo.UseShellExecute = False
            processInfo.RedirectStandardError = True

            Using process As Process = Process.Start(processInfo)
                process.WaitForExit()
                Dim errorMessage As String = process.StandardError.ReadToEnd()
                If Not String.IsNullOrEmpty(errorMessage) Then
                    MessageBox.Show("Error creating task: " & errorMessage)
                Else
                    MessageBox.Show("Task created successfully!")
                    CheckTaskStatus()
                End If
            End Using

            ' Delete the temporary XML file
            File.Delete(tempXmlFile)
        Catch ex As Exception
            MessageBox.Show("Error creating task: " & ex.Message)
        End Try
    End Sub


    Private Sub btnStop_Click(sender As Object, e As EventArgs) Handles btnStop.Click
        Try
            ' Delete the task named "scotia-email-drafts"
            Dim processInfo As New ProcessStartInfo("schtasks", "/Delete /TN ""scotia-email-drafts"" /F")
            processInfo.CreateNoWindow = True
            processInfo.UseShellExecute = False
            processInfo.RedirectStandardError = True

            Using process As Process = Process.Start(processInfo)
                process.WaitForExit()
                Dim errorMessage As String = process.StandardError.ReadToEnd()
                If Not String.IsNullOrEmpty(errorMessage) Then
                    MessageBox.Show("Error stopping task: " & errorMessage)
                Else
                    MessageBox.Show("Task stopped successfully!")
                    CheckTaskStatus()
                End If
            End Using
        Catch ex As Exception
            MessageBox.Show("Error stopping task: " & ex.Message)
        End Try
    End Sub

    Private Function TaskIsScheduled(taskName As String) As Boolean
        Dim processInfo As New ProcessStartInfo("schtasks", "/Query /TN """ & taskName & """")
        processInfo.CreateNoWindow = True
        processInfo.UseShellExecute = False
        processInfo.RedirectStandardOutput = True

        Using process As Process = Process.Start(processInfo)
            process.WaitForExit()

            ' Read the standard output to check if the task exists
            Dim output As String = process.StandardOutput.ReadToEnd()
            Return output.ToLower().Contains(taskName.ToLower())
        End Using
    End Function
    Private Function GetNextRunTime(taskName As String) As String
        Dim processInfo As New ProcessStartInfo("schtasks", "/Query /TN """ & taskName & """")
        processInfo.CreateNoWindow = True
        processInfo.UseShellExecute = False
        processInfo.RedirectStandardOutput = True

        Dim nextRunTime As String = "Not available"

        Using process As Process = Process.Start(processInfo)
            process.WaitForExit()

            ' Read the standard output to find the next run time
            Dim output As String = process.StandardOutput.ReadToEnd()

            ' Split the output into lines
            Dim lines() As String = output.Split({Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries)


            Console.WriteLine(lines.Length)

            ' Check if there are enough lines
            If lines.Length >= 4 Then
                ' Get the third line and split it by whitespace
                Dim parts() As String = lines(3).Split({" "}, StringSplitOptions.RemoveEmptyEntries)
                Console.WriteLine(parts.Length)
                ' Check if there are enough parts
                If parts.Length >= 3 Then
                    ' Join the remaining parts after removing the first two words and the last word
                    nextRunTime = String.Join(" ", parts.Skip(1).Take(parts.Length - 3))
                    Console.WriteLine(nextRunTime)
                End If
            End If
        End Using

        Return nextRunTime
    End Function

End Class
