import React, { FC } from 'react';
import { Link } from 'react-router-dom';

/** @jsx jsx */
import { css, jsx } from '@emotion/core';
import { gray2, gray3 } from '../css/Styles';

// Interfaces
import { QuestionsData } from '../interfaces/QuestionsData';

interface Props {
  QuestionsData: QuestionsData;
  showContent?: boolean; //content is param of QUESTIONDATA
}

// const Question: FC<Props> = ({ QuestionsData, showContent = true or false }) => { OPCINAL
const Question: FC<Props> = ({ QuestionsData, showContent }) => {
  // console.log(showContent);
  return (
    <div
      css={css`
        padding: 10px 0px;
      `}
    >
      <div
        css={css`
          padding: 10px 0px;
          font-size: 19px;
        `}
      >
        <Link
          css={css`
            text-decoration: none;
            color: ${gray2};
          `}
          to={`questions/${QuestionsData.questionId}`}
        >
          {QuestionsData.title}
        </Link>
      </div>
      {showContent && (
        <div
          css={css`
            padding-bottom: 10px;
            font-size: 15px;
            color: ${gray2};
          `}
        >
          {QuestionsData.content.length > 50
            ? `${QuestionsData.content.substring(0, 50)}...`
            : QuestionsData.content}
        </div>
      )}
      <div
        css={css`
          font-size: 12px;
          font-style: italic;
          color: ${gray3};
        `}
      >
        {`Asked by ${
          QuestionsData.userName
        } on ${QuestionsData.created.toLocaleDateString()} ${QuestionsData.created.toLocaleTimeString()}`}
      </div>
    </div>
  );
};
// OPTIONAL DELETE IT
Question.defaultProps = {
  showContent: true,
};

export default Question;
