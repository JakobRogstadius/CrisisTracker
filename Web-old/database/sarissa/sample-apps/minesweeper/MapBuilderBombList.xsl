<?xml version="1.0" encoding="UTF-8"?>
<!--
/**
 * ====================================================================
 * About
 * ====================================================================
 * Minesweeper
 * @version 0.9.6.1
 * @author: Copyright Sean Whalen, Manos Batsis
 *
 * This module is a port of the famous Minesweeper game in pure XSLT and JS
 *
 *	This stylesheet receives an XML list of XY values and builds an HTML table of button elements 
 * from that data.  For example, A list of 100 XY points would become a 10 by 10 table.  
 * The process uses the "make-board" template to loop through the vertical (Y-axis) points.  For each 
 * "V" value, the template calls the "make-row" template, which does the actual work 
 * of writing the HTML.  When the "make-row" routine finishes, the "make-board" template calls itself
 * using the current V value, incremented by one, as a parameter. 
 *
 *	There are two different looping styles used.  The make-board template uses recursion, 
 * and the make-row template uses a for-each loop.  With a more complicated xpath expression
 * the make-board code could have gotten a distinct list of V values, and the used a for-each loop.
 *
 * ====================================================================
 * Licence
 * ====================================================================
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License version 2 or
 * the GNU Lesser General Public License version 2.1 as published by
 * the Free Software Foundation (your choice of the two).
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License or GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * or GNU Lesser General Public License along with this program; if not,
 * write to the Free Software Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
 * or visit http://www.gnu.org
 *
 */
 -->
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
 <xsl:output method="xml" media-type="text/html"/>
 <xsl:template match="/">
  <html>
   <head>
    <xsl:text> </xsl:text>
   </head>
   <body>
    <form name="form1">
     <table border="1" bgcolor="gray">
      <xsl:call-template name="make-board"/>
     </table>
    </form>
   </body>
  </html>
 </xsl:template>
 <xsl:template name="make-board">
  <xsl:param name="markV" select="0"/>
  <xsl:for-each select="//square[@v=$markV][1]">
   <xsl:sort select="@v" data-type="number"/>
   <xsl:variable name="sendV" select="@v"/>
   <tr>
    <xsl:call-template name="make-row">
     <xsl:with-param name="thisV" select="$sendV"/>
    </xsl:call-template>
   </tr>
   <xsl:call-template name="make-board">
    <xsl:with-param name="markV" select="$markV+1"/>
   </xsl:call-template>
  </xsl:for-each>
 </xsl:template>
 <xsl:template name="make-row">
  <xsl:param name="thisV"/>
  <xsl:for-each select="//square[@v=$thisV]">
   <xsl:sort select="@h" data-type="number"/>
   <xsl:variable name="loopH" select="@h"/>
   <td>
    <input type="button" name="myButton" oncontextmenu="return false"
     style="background-color:#88AABB; border-width: 4; width: 30;">
     <xsl:attribute name="id">
      <xsl:value-of select="$loopH"/>/<xsl:value-of select="$thisV"/>
     </xsl:attribute>
     <!-- -->
     <!--	 	<xsl:attribute name="value">  -->
     <!--	 	  	<xsl:value-of select="@isBomb"/>,<xsl:value-of select="@nbc"/> -->
     <!--	 		 -->
     <!--	 	</xsl:attribute> -->
    </input>
   </td>
  </xsl:for-each>
 </xsl:template>
</xsl:stylesheet>
