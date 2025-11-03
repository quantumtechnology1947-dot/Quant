function confirmationAdd()
{
RemoveSpecialChar();
if(confirm("Are you sure you want to Add new record?")==true)
return true;
else
return false;
}

function confirmationUpdate()
{
RemoveSpecialChar();
if(confirm("Are you sure you want to Update this record?")==true)
return true;
else
return false;
document.form1.submit();
}

function confirmationDelete()
{
    if(confirm("Are you sure you want to Delete this record?")==true )
    return true;
    else
    return false;
}

function RemoveSpecialChar()
{
    theForms = document.getElementsByTagName("form"); 
        
    for(i=0;i<theForms.length;i++) 
    {
       for(j=0;j<theForms[i].elements.length;j++)
        { 
           var type1 = theForms[i].elements[j].type;
               
           if ((type1=="text" || type1=="textarea") && theForms[i].elements[j].value)
           {
               var str = theForms[i].elements[j].value; 
               var str1 = str.replace(/[~'"]/g, ' ');               
               theForms[i].elements[j].value = str1.replace(/^\s+|\s+$/g, '');
           }
        }
    }
}

function confirmation()
{
RemoveSpecialChar();
if(confirm("Are you confirm to do this Process?")==true)
return true;
else
return false;
}

function confirmationCopy()
{
RemoveSpecialChar();
if(confirm("Are you sure you want to Copy this record?")==true)
return true;
else
return false;
}
function confirmationUpload()
{
RemoveSpecialChar();
if(confirm("Are you sure you want to Upload this Drowing?")==true)
return true;
else
return false;
}

function confDyna(str)
{
RemoveSpecialChar();
if(confirm(str)==true)
return true;
else
return false;
}


function confirmationLock()
{
RemoveSpecialChar();
if(confirm("Are you sure you want to Lock Rate.")==true)
return true;
else
return false;
}

function confirmationUnLock()
{
RemoveSpecialChar();
if(confirm("Are you sure you want to Unlock Rate.")==true)
return true;
else
return false;
}