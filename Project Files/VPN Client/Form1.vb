'Creator: Tachyon
'Date: 25 Ocotber 2015
'Release Date: 27 October 2015
'License: GNU GPL v2
'If there are any issues please contact: tachyon@hackforums.org

Imports DotRas
Public Class Form1
    Dim RasCon As RasConnection
    Private Sub radL2TP_CheckedChanged(sender As Object, e As EventArgs) Handles radL2TP.CheckedChanged

        'Basically, this sub just checks if the radio button for the L2TP has changed, if so, it must enable the Pre-Shared Key box to accept the PSK to
        'complete the L2TP connection.

        If radL2TP.Checked Then
            txtPSK.Enabled = True 'Enables the TextBox if L2TP was selected
        Else
            txtPSK.Enabled = False 'Disables the TextBox when L2TP is unselected
        End If
    End Sub

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        If My.Settings.Username.Equals("") Then 'Checks to see if there is a username stored in the settings
        Else
            txtUsername.Text = My.Settings.Username 'If there is, fill the inforamtion into the textbox 
            chkRemember.Checked = True 'And check the checkbox to show that the remember function was enabled
        End If
    End Sub
    Private Sub dialer_StateChanged(sender As Object, e As StateChangedEventArgs) Handles dialer.StateChanged
        Dim li As ListViewItem 'Creates a new list view item to add items to the listview
        li = ListView1.Items.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) 'Adds the current date and time to the listview for the logs
        li.SubItems.Add(e.State.ToString) 'Adds the current state of the connection to the listview for log purposes
    End Sub

    Private Sub chkRemember_CheckedChanged(sender As Object, e As EventArgs) Handles chkRemember.CheckedChanged
        'This Sub basically checks if the checkbox for storing the users' username is checked, if it is store it, if not, dont
        If chkRemember.Checked Then
            My.Settings.Username = txtUsername.Text
        Else
            My.Settings.Username = ""
        End If
    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim creds As Net.NetworkCredential 'Creates the new NetworkCredential required to authenticate to the VPN
        Dim pptpEntry As RasEntry 'Creates an empty object of type RasEntry to be modified later for PPTP use
        Dim l2tpEntry As RasEntry 'Creates an empty object of type RasEntry to be modified later for L2TP use

        Try 'Start of Try Catch statement
            RasPhoneBook1.Open(True) 'Opens the users' default phonebook to access VPN connection information

            If txtUsername.Text = "" Then 'Makes sure that the user inputted a username
                MessageBox.Show("Please enter a username.", "No username", MessageBoxButtons.OK, MessageBoxIcon.Error) 'If there was no username entered display an error message
            Else 'If there is  username entered carry on
                If txtPassword.Text = "" Then 'Makes sure that the user inputted a password 
                    MessageBox.Show("Please enter a password.", "No password", MessageBoxButtons.OK, MessageBoxIcon.Error) 'If there was no password entered display an error
                Else 'If there is a username and password entered carry on
                    creds = New Net.NetworkCredential(txtUsername.Text, txtPassword.Text) 'Asigns the login information to the user credential object

                    If radPPTP.Checked Then 'Checks to see which VPN protocol is required: PPTP

                        For Each RasEntry In RasPhoneBook1.Entries.ToList 'Checks to see whether the VPN Connection already exists within the VPN Connection book
                            If RasEntry.Name = txtIP.Text Then 'If it finds a entry do the following:
                                dialVPN(txtIP.Text, RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User), creds) 'Dial the exisitng VPN with the user credentials the user provided
                                Exit Sub 'Exits the Sub as the connection has been started
                            End If 'End of the check of existing VPN Connections
                        Next 'Moves on to next RasEntry in the users' VPN Phonebook

                        pptpEntry = RasEntry.CreateVpnEntry(txtIP.Text, txtIP.Text, RasVpnStrategy.PptpOnly, RasDevice.GetDeviceByName("(PPTP)", RasDeviceType.Vpn, False))
                        ' The above line creates a new VPN entry for the VPN connection as no VPN connections matching the criteria have been found
                        RasPhoneBook1.Entries.Add(pptpEntry) 'Adds the VPN connection into the VPN phonebook

                        dialVPN(txtIP.Text, RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User), creds) 'Starts the VPN connection to the newly created entry

                    ElseIf radL2TP.Checked 'Checks to see which VPN protocol is required: L2TP/IPSEC

                        For Each RasEntry In RasPhoneBook1.Entries.ToList 'Checks to see whether the VPN Connection already exists within the VPN Connection book
                            If RasEntry.Name = txtIP.Text Then 'If it finds a entry do the following:
                                dialVPN(txtIP.Text, RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User), creds) 'Dial the exisitng VPN with the user credentials the user provided
                                Exit Sub 'Exits the Sub as the connection has been started
                            End If 'End of the check of existing VPN Connections
                        Next 'Moves on to next RasEntry in the users' VPN Phonebook

                        l2tpEntry = RasEntry.CreateVpnEntry(txtIP.Text, txtIP.Text, DotRas.RasVpnStrategy.L2tpOnly, RasDevice.GetDeviceByName("(L2TP)", RasDeviceType.Vpn, False))
                        ' The above line creates a new VPN entry for the VPN connection as no VPN connections matching the criteria have been found
                        l2tpEntry.Options.UsePreSharedKey = True 'This tells the VPN Entry that this connection will require a Pre-Shared Key, obviously using the L2TP protocol

                        RasPhoneBook1.Entries.Add(l2tpEntry) 'Adds this new VPN Connection into the phonebook

                        l2tpEntry.UpdateCredentials(RasPreSharedKey.Client, txtPSK.Text) 'Updates the Pre-Shared key of the newly created VPN Connection

                        dialVPN(txtIP.Text, RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User), creds) 'Starts the VPN connection to the newly created entry
                    End If 'End of the If statement to see which protocol is required
                End If 'End of the If statement to check the password field
            End If 'Enf of the If statement to check the username field
        Catch ex As Exception 'Catches any Exceptions
            MessageBox.Show(ex.Message, Me.Text, MessageBoxButtons.OK, MessageBoxIcon.Error) 'Creates a MessageBox with the error that has occured
        End Try
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        Dim li As ListViewItem 'Creates a new listview item to add information to for use late on

        RasCon = RasConnection.GetActiveConnectionByName(txtIP.Text, RasPhoneBook.GetPhoneBookPath(RasPhoneBookType.User)) 'Gets the current VPN connection
        RasCon.HangUp() 'Ends the current VPN connection

        ListView1.Items.Clear() 'Clears the listbox of all it's information

        li = ListView1.Items.Add(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")) 'Gets the current date/time and adds it to the listbox item
        li.SubItems.Add("VPN Disconnected") 'Adds the information regarding what happened to the connection to the item and adds it to the listbox
    End Sub

    Public Sub dialVPN(ByVal entryName As String, ByVal bookPath As String, ByVal credentials As Net.NetworkCredential)
        'Method to actually connect the the VPN that has been created/called
        dialer.EntryName = entryName 'Sets the dialer to the name of the connection you are trying to use
        dialer.PhoneBookPath = bookPath 'Sets the book path of where the connection information is actually stored
        dialer.Credentials = credentials 'Sets the credentials required to login the the VPN service

        dialer.DialAsync() 'Finally, it dials the VPN and connects to it (hopefully)
    End Sub
End Class