
/* * ====================================================================
 * About: This a a compressed JS file from the Sarissa library. 
 * see http://dev.abiss.gr/sarissa
 * 
 * Copyright: Manos Batsis, http://dev.abiss.gr
 * 
 * Licence:
 * Sarissa is free software distributed under the GNU GPL version 2 
 * or higher, GNU LGPL version 2.1 or higher and Apache Software 
 * License 2.0 or higher. The licenses are available online see: 
 * http://www.gnu.org  
 * http://www.apache.org
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY 
 * KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE 
 * WARRANTIES OF MERCHANTABILITY,FITNESS FOR A PARTICULAR PURPOSE 
 * AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR 
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
 * OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * ====================================================================*/

function SarissaMediaWikiContext(apiUrl,arrLanguages){this.baseUrl=apiUrl;this.format="json";this.languages=arrLanguages;};SarissaMediaWikiContext.prototype.doArticleGet=function(sFor,callback){Sarissa.setRemoteJsonCallback(this.baseUrl+"?action=parse&redirects&format="+
this.format+"&page"+
encodeURIComponent(sFor),callback);};SarissaMediaWikiContext.prototype.doBacklinksGet=function(sFor,iLimit,callback){Sarissa.setRemoteJsonCallback(this.baseUrl+"?&generator=backlinks&format="+
this.format+"&gbllimit="+
iLimit+"&gbltitle"+
encodeURIComponent(sFor),callback);};SarissaMediaWikiContext.prototype.doSearch=function(sFor,iLimit,callback){Sarissa.setRemoteJsonCallback(this.baseUrl+"?action=query&list=search&srsearch="+
encodeURIComponent(sFor)+"&srwhat=text&srnamespace=0&format="+
this.format+"&srlimit="+
iLimit,callback);};SarissaMediaWikiContext.prototype.doCategorySearch=function(sFor,iLimit,callback){Sarissa.setRemoteJsonCallback(this.baseUrl+"?format="+
this.format+"&list=categorymembers&action=query&cmlimit="+
iLimit+"&cmtitle=Category:"+
encodeURIComponent(sFor),callback);};SarissaMediaWikiContext.prototype.doArticleCategoriesGet=function(sFor,iLimit,callback){Sarissa.setRemoteJsonCallback(this.baseUrl+"?format="+
this.format+"&action=query&prop=categories&titles="+
encodeURIComponent(sFor),callback);};