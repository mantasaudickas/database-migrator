root
|__ ver0.1
|  |
|  |__ 00001. Create schema.sql
|  |__ 00002. Create first table.sql
|
|__ ver0.2
   |
   |__ 00001. Create next version.sql
   
NOTE 1: ver0.1, ver0.2 represents a concept, names can be any which are sorted using normal sorting.
      can be also like: M1, M2, M3
      or: 2015-03, 2015-04, etc.
      
NOTE 2: file names should contain version number, keep in mind that 00001 and 01 is still the same number.
        It is not allowed to have files in one folder with same version.
        Version MUST have a dot after digits.
        Invalid names: 
            a0001. Create.sql
            00001 . Create.sql
            00001a. Create.sql
            ... 
        
        In general format is: <number>. <file name>.sql