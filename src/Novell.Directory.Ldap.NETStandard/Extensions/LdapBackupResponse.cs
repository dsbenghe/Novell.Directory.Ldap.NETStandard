/******************************************************************************
* The MIT License
* Copyright (c) 2006 Novell Inc.  www.novell.com
* 
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
* copies of the Software, and to  permit persons to whom the Software is 
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in 
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
//
// Novell.Directory.Ldap.Extensions.BackupRestoreConstants.cs
//
// Author:
//   Palaniappan N (NPalaniappan@novell.com)
//
// (C) 2006 Novell, Inc (http://www.novell.com)
//

using System;
using System.IO;
using System.Text;
using Novell.Directory.Ldap.Asn1;
using Novell.Directory.Ldap.Rfc2251;

/**
 *  This object represent the data returned from a LdapBackupRequest.
 *
 *  <p>An object in this class is generated from an ExtendedResponse object
 *  using the ExtendedResponseFactory class.</p>
 *
 *  <p>The LdapBackupResponse extension uses the following OID:<br>
 *  &nbsp;&nbsp;&nbsp;2.16.840.1.113719.1.27.100.97</p>
 *
 */

namespace Novell.Directory.Ldap.Extensions
{
    public class LdapBackupResponse : LdapExtendedResponse
    {

        /*
         * The String representing the number of chunks and each elements in chunk
        * array as returned by server.
        * Data from server is parsed as follows before sending to any Application::
        * no_of_chunks;sizeOf(chunk1);sizeOf(chunk2)…sizeOf(chunkn)
        * where
        * no_of_chunks => Represents the number of chunks of data returned from server
        * sizeOf(chunkn) => Represents the size of data in chunkn
        */


        /*
         * Actual data of returned eDirectoty Object in byte[]
        */

        /**
        * Constructs an object from the responseValue which contains the backup data.
        *  <p>The constructor parses the responseValue which has the following
        *  format:<br>
        *  responseValue ::=<br>
        *  <p>databufferLength ::= INTEGER <br>
        *  mts(modification time stamp) ::= INTEGER<br>
        *  revision ::= INTEGER<br>
        *  returnedBuffer ::= OCTET STRING<br>
        *  dataChunkSizes ::= <br>
        *  SEQUENCE{<br>
        *  noOfChunks INTEGER<br>
        *  SET of [<br>
        *  SEQUENCE of {eachChunksize INTEGER}]<br>
        *  }</p>
        * 
        * @exception IOException The responseValue could not be decoded.
        */

        public LdapBackupResponse(RfcLdapMessage rfcMessage) : base(rfcMessage)
        {
            var modificationTime = 0; // Modifaction timestamp of the Object
            var revision = 0; // Revision number of the Object
            var chunksSize = 0;
            int[] chunks = null; //Holds size of each chunks returned from server

            //Verify if returned ID is not proper
            if (ID == null || !ID.Equals(BackupRestoreConstants.NLDAP_LDAP_BACKUP_RESPONSE))
                throw new IOException("LDAP Extended Operation not supported");

            if (ResultCode == LdapException.SUCCESS)
            {
                // Get the contents of the reply

                var returnedValue = Value;
                if (returnedValue == null)
                    throw new Exception("LDAP Operations error. No returned value.");

                // Create a decoder object
                var decoder = new LBERDecoder();

                // Parse the parameters in the order
                var currentPtr = new MemoryStream(returnedValue);

                // Parse bufferLength
                var asn1_bufferLength = decoder.Decode(currentPtr) as Asn1Integer;
                if (asn1_bufferLength == null)
                    throw new IOException("Decoding error");
                BufferLength = asn1_bufferLength.IntValue;

                // Parse modificationTime
                var asn1_modificationTime = decoder.Decode(currentPtr) as Asn1Integer;
                if (asn1_modificationTime == null)
                    throw new IOException("Decoding error");
                modificationTime = asn1_modificationTime.IntValue;

                // Parse revision
                var asn1_revision = decoder.Decode(currentPtr) as Asn1Integer;
                if (asn1_revision == null)
                    throw new IOException("Decoding error");
                revision = asn1_revision.IntValue;

                //Format stateInfo to contain both modificationTime and revision
                StatusInfo = modificationTime + "+" + revision;

                // Parse returnedBuffer
                var asn1_returnedBuffer = (Asn1OctetString)decoder.Decode(currentPtr);
                if (asn1_returnedBuffer == null)
                    throw new IOException("Decoding error");

                ReturnedBuffer = asn1_returnedBuffer.ByteValue;


                /* 
                 * Parse chunks array 
                 * Chunks returned from server is encoded as shown below::
                 * SEQUENCE{
                 * 			chunksSize	INTEGER
                 * 			SET of [
                 * 				SEQUENCE of {eacChunksize        INTEGER}]
                 * 	       }
                 */

                var asn1_chunksSeq = decoder.Decode(currentPtr) as Asn1Sequence;
                if (asn1_chunksSeq == null)
                    throw new IOException("Decoding error");

                //Get number of chunks returned from server
                chunksSize = (asn1_chunksSeq[0] as Asn1Integer).IntValue;

                //Construct chunks array
                chunks = new int[chunksSize];

                var asn1_chunksSet = asn1_chunksSeq[1] as Asn1Set;
                //Iterate through asn1_chunksSet and put each size into chunks array

                for (var index = 0; index < chunksSize; index++)
                {
                    var asn1_eachSeq = asn1_chunksSet[index] as Asn1Sequence;
                    chunks[index] = (asn1_eachSeq[0] as Asn1Integer).IntValue;
                }

                //Construct a temporary StringBuffer and append chunksSize, each size
                //element in chunks array and actual data of eDirectoty Object
                var tempBuffer = new StringBuilder()
                                        .Append(chunksSize)
                                        .Append(";");
                var i = 0;

                for (; i < chunksSize - 1; i++)
                {
                    tempBuffer.Append(chunks[i])
                              .Append(";");
                }

                tempBuffer.Append(chunks[i]);

                //Assign tempBuffer to parsedString to be returned to Application
                ChunkSizesString = tempBuffer.ToString();
            }
            else
            {
                //Intialize all these if getResultCode() != LdapException.SUCCESS
                BufferLength = 0;
                StatusInfo = null;
                ChunkSizesString = null;
                ReturnedBuffer = null;
            }
        }

        /**
         * Returns the data buffer length
         *
         * @return bufferLength as integer.
         */

        public int BufferLength { get; }

        /**
         * Returns the stateInfo of returned eDirectory Object.
         * This is combination of MT (Modification Timestamp) and
         * Revision value with char '+' as separator between two.<br>
         * Client application if want to use both MT and Revision need to break
         * this string to get both these data.
         *
         * @return stateInfo as String.
         */

        public string StatusInfo { get; }

        /// <summary>
        /// Returns the data in String as 
        /// no_of_chunks;sizeOf(chunk1);sizeOf(chunk2)…sizeOf(chunkn)
        /// no_of_chunks => Represents the number of chunks of data returned from server
        /// sizeOf(chunkn) => Represents the size of data in chunkn
        /// </summary>
        public string ChunkSizesString { get; }

        /**
         * Returns the data buffer as byte[]
         *
         * @return returnedBuffer as byte[].
         */

        public byte[] ReturnedBuffer { get; }
    }
}