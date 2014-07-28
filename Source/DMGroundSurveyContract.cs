﻿#region license
/* DMagic Orbital Science - DMGroundSurveyContract
 * Class for generating surface science experiment contracts
 *
 * Copyright (c) 2014, David Grandy <david.grandy@gmail.com>
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without modification, 
 * are permitted provided that the following conditions are met:
 * 
 * 1. Redistributions of source code must retain the above copyright notice, 
 * this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright notice, 
 * this list of conditions and the following disclaimer in the documentation and/or other materials 
 * provided with the distribution.
 * 
 * 3. Neither the name of the copyright holder nor the names of its contributors may be used 
 * to endorse or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, 
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE 
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, 
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF 
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT 
 * OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *  
 */
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Contracts;
using Contracts.Parameters;
using Contracts.Agents;

namespace DMagic
{
	class DMGroundSurveyContract: Contract
	{
		internal DMCollectScience[] newParams = new DMCollectScience[5];
		private LandOnBody landParam;
		private CelestialBody body;
		private int i = 0;
		private System.Random rand = DMUtils.rand;

		protected override bool Generate()
		{
			if (!GetBodies_Reached(true, true).Contains(FlightGlobals.Bodies[1]))
				return false;
			if (ContractSystem.Instance.GetCurrentContracts<DMGroundSurveyContract>().Count() > 0)
				return false;

			//Generates the science experiment, returns null if experiment fails any check
			if ((newParams[0] = DMSurveyGenerator.fetchSurveyScience(this.Prestige, GetBodies_Reached(false, true), GetBodies_NextUnreached(4, null), 1)) == null)
				return false;

			body = newParams[0].Body;

			//Generate several more experiments using the target body returned from the first; needs at least 3
			if ((newParams[1] = DMSurveyGenerator.fetchSurveyScience(body, 1)) == null)
				return false;
			if ((newParams[2] = DMSurveyGenerator.fetchSurveyScience(body, 1)) == null)
				return false;
			newParams[3] = DMSurveyGenerator.fetchSurveyScience(body, 1);
			newParams[4] = DMSurveyGenerator.fetchSurveyScience(body, 1);

			//Add a landing parameter
			landParam = new LandOnBody(body);
			this.AddParameter(landParam, null);

			//Add in all acceptable paramaters to the contract
			foreach (DMCollectScience DMC in newParams)
			{
				if (DMC != null)
				{
					this.AddParameter(newParams[i], null);
					DMUtils.DebugLog("Ground Survey Parameter Added");
				}
				i++;
			}

			int a = rand.Next(0, 5);
			if (a == 0)
				this.agent = Contracts.Agents.AgentList.Instance.GetAgent("DMagic");
			else if (a == 1)
				this.agent = Contracts.Agents.AgentList.Instance.GetAgent(newParams[0].Container.agent);
			else
				this.agent = Contracts.Agents.AgentList.Instance.GetAgentRandom();

			base.SetExpiry(10, Math.Max(15, 15) * (float)(this.prestige + 1));
			base.SetDeadlineDays(20f * (float)(this.prestige + 1), body);
			base.SetReputation(newParams.Length * body.scienceValues.InSpaceLowDataValue * 0.5f, body);
			base.SetFunds(3000 * newParams.Length * body.scienceValues.InSpaceLowDataValue, 3000 * newParams.Length, 1000 * newParams.Length * body.scienceValues.InSpaceLowDataValue, body);
			return true;
		}

		public override bool CanBeCancelled()
		{
			return true;
		}

		public override bool CanBeDeclined()
		{
			return true;
		}

		protected override string GetHashString()
		{
			return body.name;
		}

		protected override string GetTitle()
		{
			return string.Format("Conduct ground surface survey of {0} by collecting multiple scienctific observations", body.theName);
		}

		protected override string GetDescription()
		{
			//Return a random orbital survey backstory; use the same format as generic backstory
			string story = DMUtils.surveyStoryList[rand.Next(0, DMUtils.surveyStoryList.Count)];
			return string.Format(story, this.agent.Name, "surface", body.theName);
		}

		protected override string GetSynopsys()
		{
			DMUtils.DebugLog("Generating Ground Synopsis From Target Body: [{0}]", body.theName);
			return string.Format("Conduct an surface survey of {0} by collecting multiple science observations.", body.theName);
		}

		protected override string MessageCompleted()
		{
			return string.Format("You completed a survey of {0}, well done.", body.theName);
		}

		protected override void OnLoad(ConfigNode node)
		{
			DMUtils.DebugLog("Loading Ground Survey Contract");
			int target;
			target = int.Parse(node.GetValue("Surface_Survey_Target"));
			body = FlightGlobals.Bodies[target];
		}

		protected override void OnSave(ConfigNode node)
		{
			DMUtils.DebugLog("Saving Ground Survey Contract");
			node.AddValue("Surface_Survey_Target", body.flightGlobalsIndex);
		}

		public override bool MeetRequirements()
		{
			return true;
		}
	}
}
