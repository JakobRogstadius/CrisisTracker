<?xml version="1.0" encoding="UTF-8"?>
<!-- 
 /**
 * ====================================================================
 * About
 * ====================================================================
 * All XSLT Minesweeper
 * @version @sarissa.version@
 * @author: Copyright Sean Whalen
 * 
 *     This stylesheet is an implementation of a method for constructing a complete XML tree 
 * of X/Y points from an initial pair of Maximum values.  The code uses H and V as 
 * abbreviations for horizontal and vertical.  The list that is generated starts at 0/0 
 * and runs through hMax/vMax.  It also has the ability to give special attribute values to HV pairs 
 * that are passed in with the two maximums.  
 * 	The implementation uses two recursive templates named "make-list" and "make-row" to build
 * the list of coordinates.
 * The "make-list" template has a counter that maintains the number of rows that have been made. 
 * The counter is intialized to zero.  If the counter is less than the maximum number of rows, 
 * it calls the "make-row" template.  In this implementation the maximum number of rows is synonymous 
 * with the vertical size of the grid, so that value is stored in the vMax variable.  When that call returns,
 * the "make-list" routine calls itself, and passes as a parameter the incremented counter.  The 
 * recursive looping will continue until the template receives as an argument a value that is greater
 * than the vertical maximum that it originally received.  This approach was used because
 * variables in XSLT cannot be changed once they have been created.  A general approach to looping is to 
 * use a template to contain the code that will be executed repetitively.  The template should have as 
 * parameters a control variable and a counter variable.  After the last line of real work in the template, 
 * the template should call itself using as an argument an equation that increments
 *  the counter variable.
 *	The "make-row" template uses a similar recursive looping strategy to create the correct
 * number of elements.  It receives the "hMax" variable as a parameter, and uses that to control 
 * its recursive loop.  
 *	The "make-row" template is also where special attribute values are assigned to elements that have 
 * h/v values that match the set that originally delivered to the stylesheet.   
 * In this particular implementation, the attribute "isBomb" is set to "-1" if the current H and V values 
 * being written into the output tree match a pair that was in the source XML.  The attribute "nbc" is
 * an abbreviation for "neighboringBombCount"; it is the number of bordering elements that are bombs.  
 * Technically, "neighboringBombCount" is the number of elements in the source XML that have
 * the attribute "isBomb" is set to "-1" and that have H and V values that are at most
 * that are at most 1 different from the element currently being written
 * e.g., ( (abs(Hi-Hj) BETWEEN 0 and 1) and (abs(Vi-Vj) BETWEEN 0 and 1) ).
 *
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
  <xsl:template match="/">
    <xsl:element name="SweeperMap">
      <xsl:call-template name="make-list"/>
    </xsl:element>
  </xsl:template>
  <xsl:template name="make-list">
    <xsl:param name="thisH" select="0"/>
    <xsl:param name="thisV" select="0"/>
    <xsl:variable name="vMax" select="//range/@vMax"/>
    <xsl:variable name="hMax" select="//range/@hMax"/>
    <xsl:if test="($thisV)!=$vMax">
      <xsl:call-template name="make-row">
        <xsl:with-param name="rowH" select="$thisH"/>
        <xsl:with-param name="rowV" select="$thisV"/>
        <xsl:with-param name="hMax" select="$hMax"/>
      </xsl:call-template>
      <xsl:call-template name="make-list">
        <xsl:with-param name="thisH" select="0"/>
        <xsl:with-param name="thisV" select="$thisV+1"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
  <xsl:template name="make-row">
    <xsl:param name="rowH"/>
    <xsl:param name="rowV"/>
    <xsl:param name="hMax"/>
    <xsl:variable name="neighboringBombCount" select="count(//bomb[@h=$rowH +1 and @v=$rowV    ])
      +  count(//bomb[@h=$rowH +1 and @v=$rowV +1 ])        +  count(//bomb[@h=$rowH +1 and @v=$rowV
      -1 ])        +  count(//bomb[@h=$rowH -1 and @v=$rowV    ])       +  count(//bomb[@h=$rowH -1
      and @v=$rowV +1 ])        +  count(//bomb[@h=$rowH -1 and @v=$rowV -1 ])        +
      count(//bomb[@h=$rowH    and @v=$rowV -1 ])        + count(//bomb[@h=$rowH    and @v=$rowV +1
      ])          "/>
    <xsl:element name="square">
      <xsl:attribute name="h">
        <xsl:value-of select="$rowH"/>
      </xsl:attribute>
      <xsl:attribute name="v">
        <xsl:value-of select="$rowV"/>
      </xsl:attribute>
      <xsl:attribute name="sqID">
        <xsl:value-of select="$rowH"/>/<xsl:value-of select="$rowV"/>
      </xsl:attribute>
      <xsl:attribute name="isRevealed">
        <xsl:value-of select="0"/>
      </xsl:attribute>
      <xsl:attribute name="nbc">
        <xsl:value-of select="$neighboringBombCount"/>
      </xsl:attribute>
      <xsl:if test="//bomb[@h=$rowH and @v=$rowV]">
        <xsl:attribute name="isBomb">
          <xsl:value-of select="-1"/>
        </xsl:attribute>
      </xsl:if>
      <xsl:if test="not(//bomb[@h=$rowH and @v=$rowV])">
        <xsl:attribute name="isBomb">
          <xsl:value-of select="0"/>
        </xsl:attribute>
      </xsl:if>
    </xsl:element>
    <xsl:if test="$rowH &lt; $hMax -1">
      <xsl:call-template name="make-row">
        <xsl:with-param name="rowH" select="$rowH+1"/>
        <xsl:with-param name="rowV" select="$rowV"/>
        <xsl:with-param name="hMax" select="$hMax"/>
      </xsl:call-template>
    </xsl:if>
  </xsl:template>
</xsl:stylesheet>
