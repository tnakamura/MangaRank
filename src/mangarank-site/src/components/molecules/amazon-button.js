import React from 'react'
import { Button } from 'reactstrap'
import AmazonIcon from '../atoms/amazon-icon'

const AmazonButton = (props) => {
  const {
    detailPageUrl,
    ...attributes
  } = props

  return (
    <Button tag="a"
            href={detailPageUrl}
            target="_blank"
            rel="noopener noreferrer"
            {...attributes}
            color="warning">
      <AmazonIcon />
      Amazon で購入
    </Button>
  )
}

export default AmazonButton
